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
                var todayString = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var today = DateTime.ParseExact(todayString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                
                var paimtos = _parkingBotDbContext.Logs.Where(x => x.Action == "imu" && x.DateTo >= today).ToList();
                paimtos = paimtos.OrderByDescending(o => o.DateFrom).Take(25).ToList();
               
                var laisvos = _parkingBotDbContext.Logs.Where(x => x.Action == "laisva").ToList();
                
                var rows = new List<Row>
                {
                    new Row
                    {
                        Column = new List<string>
                        {
                            "VARTOTOJAS",
                            "IVAZIAVIMAS",
                            "VIETA",
                            "NUO",
                            "IKI"
                        }
                    }
                };

                foreach (var paimta in paimtos)
                {
                    var compromised = false;
                    
                    foreach (var laisva in laisvos)
                    {
                        if (paimta.UserName == laisva.UserName && paimta.Location == laisva.Location &&
                            paimta.Spot == laisva.Spot && paimta.DateFrom == laisva.DateFrom && paimta.DateTo == laisva.DateTo)
                        {
                            compromised = true;
                        }
                    }

                    if (!compromised)
                    {
                        rows.Add(new Row
                        {
                            Column = new List<string>
                            {
                                paimta.UserName,
                                paimta.Location,
                                paimta.Spot,
                                paimta.DateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                                paimta.DateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                            }
                        });
                    }
                }
                
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