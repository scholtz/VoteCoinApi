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
        private readonly PriceRepository priceRepository;
        public PriceController(PriceRepository priceRepository)
        {
            this.priceRepository = priceRepository;
        }
        [ResponseCache(Duration = 60)]
        [HttpGet("Get/{fromToken}/{toToken}")]
        public async Task<ActionResult<decimal>> Get([FromRoute] ulong fromToken, [FromRoute] ulong toToken)
        {
            try
            {
                return Ok(await priceRepository.Get(fromToken, toToken));
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }
    }
}