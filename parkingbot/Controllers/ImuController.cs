using Microsoft.AspNetCore.Mvc;

namespace parkingbot.Controllers
{
    [Route("api/[controller]")]
    public class ImuController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "ImuController Works!";
        }

        [HttpPost]
        public void Post([FromBody] string value)
        {

        }
    }
}