using Algorand;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace VoteCoinApi.Repository
{
    [Route("[controller]")]
    public class PriceRepository
    {
        private readonly ILogger<PriceRepository> _logger;
        private readonly Algorand.Algod.DefaultApi algodClient;
        private readonly Algorand.Indexer.LookupApi lookupClient;
        //private readonly Tinyman.V1.TinymanMainnetClient tinymanMainnetClient;
        //private readonly Dictionary<ulong, Tinyman.V1.Model.Asset> assets;
        //private ConcurrentDictionary<string, (DateTimeOffset Time, Tinyman.V1.Model.Pool pool)> cachePrices = new ConcurrentDictionary<string, (DateTimeOffset, Tinyman.V1.Model.Pool)>();

        public PriceRepository(
            ILogger<PriceRepository> logger,
            IOptionsMonitor<Model.Config.AlgodConfig> algodConfig,
            IOptionsMonitor<Model.Config.IndexerConfig> indexerConfig
            )
        {
            _logger = logger;

            var algodHttpClient = HttpClientConfigurator.ConfigureHttpClient(algodConfig.CurrentValue.Host, algodConfig.CurrentValue.Token, algodConfig.CurrentValue.Header);

            algodClient = new Algorand.Algod.DefaultApi(algodHttpClient);


            var indexerHttpClient = HttpClientConfigurator.ConfigureHttpClient(indexerConfig.CurrentValue.Host, indexerConfig.CurrentValue.Token, indexerConfig.CurrentValue.Header);

            lookupClient = new Algorand.Indexer.LookupApi(indexerHttpClient);
            //tinymanMainnetClient = new Tinyman.V1.TinymanMainnetClient(algodClient, lookupClient);

            //assets = new Dictionary<ulong, Tinyman.V1.Model.Asset>()
            //{
            //    [452399768] = tinymanMainnetClient.FetchAssetAsync(452399768).Result,
            //    [31566704] = tinymanMainnetClient.FetchAssetAsync(31566704).Result,
            //    [0] = tinymanMainnetClient.FetchAssetAsync(0).Result,
            //};

        }
        //private decimal MidPrice(Tinyman.V1.Model.Pool pool, Tinyman.V1.Model.Asset fromToken, Tinyman.V1.Model.Asset toToken)
        //{
        //    ulong q = 1000000;
        //    var from = pool.CalculateFixedInputSwapQuote(new Tinyman.V1.Model.AssetAmount(fromToken, q), 0.00);
        //    var price1 = Convert.ToDecimal(q) / Convert.ToDecimal(from.AmountOut.Amount);

        //    var to = pool.CalculateFixedInputSwapQuote(new Tinyman.V1.Model.AssetAmount(toToken, q), 0.00);
        //    var price2 = Convert.ToDecimal(to.AmountOut.Amount) / Convert.ToDecimal(q);

        //    return (price1 + price2) / 2;
        //}
        //public async Task<decimal> Get(ulong fromToken, ulong toToken)
        //{
        //    if (assets.TryGetValue(fromToken, out var from))
        //    {

        //        if (assets.TryGetValue(toToken, out var to))
        //        {
        //            var cacheKey = $"{fromToken}-{toToken}";
        //            if (cachePrices.TryGetValue(cacheKey, out var cache))
        //            {
        //                if (cache.Time.AddMinutes(1) > DateTimeOffset.Now)
        //                {
        //                    return MidPrice(cache.pool, from, to);
        //                }
        //            }
        //            var ret = await tinymanMainnetClient.FetchPoolAsync(from, to);
        //            cachePrices[$"{fromToken}-{toToken}"] = (DateTimeOffset.Now, ret);
        //            return MidPrice(ret, from, to);
        //        }
        //        else
        //        {
        //            throw new Exception($"Token {toToken} not found");
        //        }
        //    }
        //    else
        //    {
        //        throw new Exception($"Token {fromToken} not found");
        //    }
        //}

        internal async Task<ulong> CirculationSupply(string[] voteCoinDAOAddressesList)
        {
            ulong ret = 1000000000000000L;
            foreach (var address in voteCoinDAOAddressesList)
            {
                var account = await lookupClient.lookupAccountByIDAsync(address);
                var asset = account.Account?.Assets?.FirstOrDefault(a => a.AssetId == 452399768);
                if(asset?.Amount > 0)
                {
                    ret -= asset.Amount;
                }
            }
            return Convert.ToUInt64(ret);
        }
    }
}