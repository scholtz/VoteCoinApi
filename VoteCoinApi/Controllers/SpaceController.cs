using Microsoft.AspNetCore.Mvc;
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

        [HttpGet(Name = "List")]
        public ActionResult<List<Space>> List()
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
    }
}