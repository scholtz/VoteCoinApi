using Algorand.V2;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using VoteCoinApi.Model;
using VoteCoinApi.Repository;

namespace VoteCoinApi.Controllers
{
    [EnableCors("VoteCoinPolicy")]
    [ApiController]
    [Route("[controller]")]
    public class PriceController : ControllerBase
    {
        private readonly ILogger<SpaceController> _logger;
        private readonly Algorand.V2.Algod.DefaultApi algodClient;
        private readonly Tinyman.V1.TinymanMainnetClient tinymanMainnetClient;
        private readonly Dictionary<ulong, Tinyman.V1.Model.Asset> assets;

        public PriceController(ILogger<SpaceController> logger, IOptionsMonitor<Model.Config.AlgodConfig> algodConfig)
        {
            _logger = logger;

            var algodHttpClient = HttpClientConfigurator.ConfigureHttpClient(algodConfig.CurrentValue.Host, algodConfig.CurrentValue.Token, algodConfig.CurrentValue.Header);

            algodClient = new Algorand.V2.Algod.DefaultApi(algodHttpClient)
            {
                BaseUrl = algodConfig.CurrentValue.Host,
            };
            tinymanMainnetClient = new Tinyman.V1.TinymanMainnetClient(algodClient);

            assets = new Dictionary<ulong, Tinyman.V1.Model.Asset>()
            {
                [452399768] = tinymanMainnetClient.FetchAssetAsync(452399768).Result,
                [31566704] = tinymanMainnetClient.FetchAssetAsync(31566704).Result,
                [0] = tinymanMainnetClient.FetchAssetAsync(0).Result,
            };

        }
        private ConcurrentDictionary<string, (DateTimeOffset Time, Tinyman.V1.Model.Pool pool)> cachePrices = new ConcurrentDictionary<string, (DateTimeOffset, Tinyman.V1.Model.Pool)>();
        [HttpGet("Get/{fromToken}/{toToken}")]
        public async Task<ActionResult<Tinyman.V1.Model.Pool>> Get([FromRoute] ulong fromToken, [FromRoute] ulong toToken)
        {
            try
            {
                if (assets.TryGetValue(fromToken, out var from))
                {

                    if (assets.TryGetValue(toToken, out var to))
                    {
                        var cacheKey = $"{fromToken}-{toToken}";
                        if (cachePrices.TryGetValue(cacheKey, out var cache))
                        {
                            if (cache.Time.AddMinutes(1) > DateTimeOffset.Now)
                            {
                                return cache.pool;
                            }
                        }
                        var ret = await tinymanMainnetClient.FetchPoolAsync(from, to);
                        cachePrices[$"{fromToken}-{toToken}"] = (DateTimeOffset.Now, ret);
                        return Ok(ret);
                    }
                    else
                    {
                        throw new Exception($"Token {toToken} not found");
                    }
                }
                else
                {
                    throw new Exception($"Token {fromToken} not found");
                }
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }
    }
}