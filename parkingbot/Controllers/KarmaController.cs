using Microsoft.AspNetCore.Mvc;

namespace parkingbot.Controllers
{
    [Route("api/[controller]")]
    public class KarmaController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "KarmaController Works!";
        }

        [HttpPost]
        public void Post([FromBody] string value)
        {

        }
    }
}