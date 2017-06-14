using System.IO;
using Microsoft.AspNetCore.Mvc;
using parkingbot.Models;

namespace parkingbot.Controllers
{
    [Route("slack/[controller]")]
    public class DebugController : Controller
    {
        [HttpPost]
        public JsonResult Post()
        {
            var postData = new StreamReader(Request.Body).ReadToEnd();
            
            return Json(new Response
            {
                ResponseType = "ephemeral",
                Text = "```" + postData + "```"
            });
        }
    }
}