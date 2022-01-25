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
        [HttpGet("{assetId}/Icon.svg")]
        public ActionResult GetImage([FromRoute] ulong assetId)
        {
            try
            {
                var icon = spaceRepository.Icon(assetId);
                return File(icon, "image/svg+xml");
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }
    }
}