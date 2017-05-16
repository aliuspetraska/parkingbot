using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using parkingbot.Models;
using parkingbot.Services;

namespace parkingbot.Controllers
{
    [Route("api/[controller]")]
    public class KarmaController : Controller
    {
        private readonly ParkingBotDbContext _parkingBotDbContext;
        private readonly GeneratorService _generator;
        private readonly ValidationService _validation;

        public KarmaController(ParkingBotDbContext parkingBotDbContext = null)
        {
            _parkingBotDbContext = parkingBotDbContext;
            _generator = new GeneratorService();
            _validation = new ValidationService();
        }

        [HttpPost]
        public JsonResult Post()
        {
            var postData = _validation.ParsePostData(new StreamReader(Request.Body).ReadToEnd());

            if (_parkingBotDbContext != null)
            {
                var karmaPoints = _parkingBotDbContext.Logs.Where(x => x.Action.ToUpper() == "LAISVA").ToList()
                    .GroupBy(u => u.UserName)
                    .Select(g => new Karma
                        {
                            UserName = g.Key,
                            KarmaPoints = g.Count()
                        }
                    ).ToList();

                return Json(new Response
                {
                    ResponseType = "ephemeral",
                    Text = "```" + _generator.GenerateKarmaTable(karmaPoints) + "```"
                });
            }

            return Json(new Response
            {
                ResponseType = "ephemeral",
                Text = "```Soriukas, nepaėjo. Bandyk dar kartą.```",
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Text = _generator.ReturnWhatYouTyped(postData)
                    }
                }
            });
        }
    }
}