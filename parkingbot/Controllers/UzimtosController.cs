using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using parkingbot.Models;
using parkingbot.Services;

namespace parkingbot.Controllers
{
    [Route("slack/[controller]")]
    public class UzimtosController : Controller
    {
        private readonly ParkingBotDbContext _parkingBotDbContext;
        private readonly GeneratorService _generator;
        private readonly ValidationService _validation;

        public UzimtosController(ParkingBotDbContext parkingBotDbContext = null)
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
                var takenSpots = _parkingBotDbContext.Logs.Where(x => x.Action.ToUpper() == "IMU" && x.DateFrom >= DateTime.Today)
                    .OrderBy(o => o.DateFrom).ToList();

                var rows = new List<Row>
                {
                    new Row
                    {
                        Column = new List<string>
                        {
                            "USERNAME",
                            "IVAZIAVIMAS",
                            "VIETA",
                            "NUO",
                            "IKI"
                        }
                    }
                };

                rows.AddRange(takenSpots.Select(item => new Row
                {
                    Column = new List<string>
                    {
                        item.UserName,
                        item.Location,
                        item.Spot,
                        item.DateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        item.DateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
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