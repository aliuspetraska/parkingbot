using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using parkingbot.Models;

namespace parkingbot.Controllers
{
    [Route("slack/[controller]")]
    public class DebugController : Controller
    {
        private readonly ParkingBotDbContext _parkingBotDbContext;

        public DebugController(ParkingBotDbContext parkingBotDbContext = null)
        {
            _parkingBotDbContext = parkingBotDbContext;
        }

        [HttpPost]
        public JsonResult Post()
        {
            var postData = new StreamReader(Request.Body).ReadToEnd();
            var dbStatus = _parkingBotDbContext != null ? "DB OK!" : "DB ERROR!";

            return Json(new Response
            {
                ResponseType = "ephemeral",
                Text = "```" + postData + "```",
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Text = dbStatus
                    }
                }
            });
        }
    }
}