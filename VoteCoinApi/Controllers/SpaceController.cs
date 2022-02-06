using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using VoteCoinApi.Model;
using VoteCoinApi.Repository;

namespace VoteCoinApi.Controllers
{
    [EnableCors("VoteCoinPolicy")]
    [ApiController]
    [Route("[controller]")]
    public class SpaceController : ControllerBase
    {
        private readonly ILogger<SpaceController> _logger;
        private readonly SpaceRepository spaceRepository;
        private readonly TransactionRepository transactionRepository;
        public SpaceController(ILogger<SpaceController> logger, SpaceRepository spaceRepository, TransactionRepository transactionRepository)
        {
            _logger = logger;
            this.spaceRepository = spaceRepository;
            this.transactionRepository = transactionRepository;
        }
        /// <summary>
        /// List tokens by environment
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("{env}/List")]
        //[ResponseCache(Duration = 3600 * 1)]
        public ActionResult<IEnumerable<SpaceBase>> List([FromRoute] string env)
        {
            try
            {
                return Ok(spaceRepository.List(env));
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }
        /// <summary>
        /// Returns svg asa icon
        /// </summary>
        /// <param name="env"></param>
        /// <param name="assetId"></param>
        /// <returns></returns>
        [ResponseCache(Duration = 3600 * 24 * 7)]
        [HttpGet("{env}/{assetId}/Icon.svg")]
        public ActionResult GetImage([FromRoute] string env, [FromRoute] ulong assetId)
        {
            try
            {
                var icon = spaceRepository.Icon(env, assetId);
                if (icon == null || icon.Length == 0)
                {
                    throw new Exception("Asset not found");
                }
                var iconMime = spaceRepository.IconMime(env, assetId);
                if (string.IsNullOrEmpty(iconMime))
                {
                    throw new Exception("Asset icon mime type not found");
                }
                return File(icon, iconMime);
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }
        /// <summary>
        /// Returns png icon
        /// </summary>
        /// <param name="env"></param>
        /// <param name="assetId"></param>
        /// <returns></returns>
        [ResponseCache(Duration = 3600 * 24 * 7)]
        [HttpGet("{env}/{assetId}/Icon.png")]
        public ActionResult GetImagePng([FromRoute] string env, [FromRoute] ulong assetId)
        {
            try
            {
                var icon = spaceRepository.Icon(env, assetId);
                if (icon == null || icon.Length == 0)
                {
                    throw new Exception("Asset not found");
                }
                var iconMime = spaceRepository.IconMime(env, assetId);
                if (string.IsNullOrEmpty(iconMime))
                {
                    throw new Exception("Asset icon mime type not found");
                }
                return File(icon, iconMime);
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }


        [HttpGet("{env}/{assetId}/Delegations")]
        public ActionResult Delegations([FromRoute] string env, [FromRoute] ulong assetId)
        {
            try
            {
                return Ok(transactionRepository.ListDelegations(env, assetId));
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }
        [HttpGet("{env}/{assetId}/Questions")]
        public ActionResult ListQuestions([FromRoute] string env, [FromRoute] ulong assetId)
        {
            try
            {
                return Ok(transactionRepository.ListQuestions(env, assetId));
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }
        [HttpGet("{env}/{assetId}/TrustedListTxs")]
        public ActionResult TrustedList([FromRoute] string env, [FromRoute] ulong assetId)
        {
            try
            {
                return Ok(transactionRepository.AssetsTrustedListTxs(env, assetId));
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }
        [HttpGet("{env}/{assetId}/Votes")]
        public ActionResult AssetsVoteTxs([FromRoute] string env, [FromRoute] ulong assetId)
        {
            try
            {
                return Ok(transactionRepository.AssetsVoteTxs(env, assetId));
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }
    }
}