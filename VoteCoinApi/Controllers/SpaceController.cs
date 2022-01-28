using Microsoft.AspNetCore.Mvc;
using System.Text;
using VoteCoinApi.Model;
using VoteCoinApi.Repository;

namespace VoteCoinApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpaceController : ControllerBase
    {
        private readonly ILogger<SpaceController> _logger;
        private readonly SpaceRepository spaceRepository;
        public SpaceController(ILogger<SpaceController> logger, SpaceRepository spaceRepository)
        {
            _logger = logger;
            this.spaceRepository = spaceRepository;
        }

        [HttpGet("List")]
        [ResponseCache(Duration = 3600 * 1)]
        public ActionResult<IEnumerable<SpaceBase>> List()
        {
            try
            {
                return Ok(spaceRepository.List());
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }
        [ResponseCache(Duration = 3600 * 24 * 7)]
        [HttpGet("{assetId}/Icon.svg")]
        public ActionResult GetImage([FromRoute] ulong assetId)
        {
            try
            {
                var icon = spaceRepository.Icon(assetId);
                if (icon == null || icon.Length == 0)
                {
                    throw new Exception("Asset not found");
                }
                var iconMime = spaceRepository.IconMime(assetId);
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
        [ResponseCache(Duration = 3600 * 24 * 7)]
        [HttpGet("{assetId}/Icon.png")]
        public ActionResult GetImagePng([FromRoute] ulong assetId)
        {
            try
            {
                var icon = spaceRepository.Icon(assetId);
                if (icon == null || icon.Length == 0)
                {
                    throw new Exception("Asset not found");
                }
                var iconMime = spaceRepository.IconMime(assetId);
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
    }
}