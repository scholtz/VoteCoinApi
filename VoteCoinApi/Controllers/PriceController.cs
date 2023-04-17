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
        //[ResponseCache(Duration = 60)]
        //[HttpGet("Get/{fromToken}/{toToken}")]
        //public async Task<ActionResult<decimal>> Get([FromRoute] ulong toToken, [FromRoute] ulong fromToken)
        //{
        //    try
        //    {
        //        return Ok(Math.Round(await priceRepository.Get(fromToken, toToken), 6));
        //    }
        //    catch (Exception exc)
        //    {
        //        return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
        //    }
        //}
        /// <summary>
        /// Returns total number of tokens. Total supply will not change.
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 60)]
        [HttpGet("TotalCirculationSupply")]
        public ActionResult<ulong> TotalCirculationSupply()
        {
            try
            {
                return Ok(1000000000000000L);
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }

        private string[] VoteCoinDAOAddressesList = new string[]
        {
            "P65LXHA5MEDMOJ2ZAITLZWYSU6W25BF2FCXJ5KQRDUB2NT2T7DPAAFYT3U",
            "VOTESZMB66LO6CGVREQENOKIBMW4JG2BA7HJUXZBAYDLE6RKM2CQ2YI5EI",
            "VOTEKDWXJ2V6PL6BYW5OCHQNJ3D77QQVYYIWO4APV3XXKVZW23WBUWPA3M"
        };

        /// <summary>
        /// Returns total number of tokens. Total supply will not change.
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 60)]
        [HttpGet("VoteCoinDAOAddresses")]
        public ActionResult<IEnumerable<string>> VoteCoinDAOAddresses()
        {
            try
            {
                return Ok(VoteCoinDAOAddressesList);
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }


        /// <summary>
        /// Returns total number of tokens in public holding - not on VoteCoin DAO balance sheet.
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 60)]
        [HttpGet("VoteCoinCirculationSupply")]
        public async Task<ActionResult<ulong>> VoteCoinCirculationSupply()
        {
            try
            {
                return Ok(await priceRepository.CirculationSupply(VoteCoinDAOAddressesList));
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }

    }
}