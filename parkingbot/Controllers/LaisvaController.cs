using Microsoft.AspNetCore.Mvc;

namespace parkingbot.Controllers
{
    [Route("api/[controller]")]
    public class LaisvaController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "LaisvaController Works!";
        }

        [HttpPost]
        public void Post([FromBody] string value)
        {

        }
    }
}