using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using parkingbot.Models;
using parkingbot.Services;

namespace parkingbot.Controllers
{
    [Route("slack/[controller]")]
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
                var query = _parkingBotDbContext.Logs.Where(x => x.Action.ToUpper() == "LAISVA").ToList();
                var karmaPoints = query.GroupBy(u => u.UserName)
                    .Select(g => new Karma
                        {
                            UserName = g.Key,
                            KarmaPoints = g.Count()
                        }
                    ).OrderByDescending(o => o.KarmaPoints).ToList();

                var rows = new List<Row>
                {
                    new Row
                    {
                        Column = new List<string>
                        {
                            "#",
                            "VARTOTOJAS",
                            "KARMA"
                        }
                    }
                };

                rows.AddRange(karmaPoints.Select((t, i) => new Row
                {
                    Column = new List<string>
                    {
                        (i + 1).ToString(),
                        t.UserName,
                        "+" + t.KarmaPoints * 1000
                    }
                }));

                return Json(new Response
                {
                    ResponseType = "ephemeral",
                    Text = "```" + _generator.GenerateTable(rows) + "```"
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