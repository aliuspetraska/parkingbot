using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using parkingbot.Models;

namespace parkingbot.Controllers
{
    [Route("api/[controller]")]
    public class DebugController : Controller
    {
        [HttpPost]
        public JsonResult Post()
        {
            var postData = new StreamReader(Request.Body).ReadToEnd();

            return Json(new Response
            {
                ResponseType = "ephemeral",
                Text = "```" + postData + "```",
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Text = "DEBUG"
                    }
                }
            });
        }
    }
}