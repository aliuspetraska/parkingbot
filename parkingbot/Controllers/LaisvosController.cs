using Microsoft.AspNetCore.Mvc;

namespace parkingbot.Controllers
{
    [Route("api/[controller]")]
    public class LaisvosController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "LaisvosController Works!";
        }

        [HttpPost]
        public void Post([FromBody] string value)
        {

        }
    }
}