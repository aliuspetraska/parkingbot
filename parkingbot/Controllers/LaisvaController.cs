using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using parkingbot.Models;
using parkingbot.Services;

namespace parkingbot.Controllers
{
    [Route("api/[controller]")]
    public class LaisvaController : Controller
    {
        private readonly ValidationService _validation;
        private readonly GeneratorService _generator;
        private readonly ParkingBotDbContext _parkingBotDbContext;

        public LaisvaController(ParkingBotDbContext parkingBotDbContext = null)
        {
            _validation = new ValidationService();
            _generator = new GeneratorService();
            _parkingBotDbContext = parkingBotDbContext;
        }

        [HttpPost]
        public JsonResult Post([FromBody] string value)
        {
            var postData = _validation.ParsePostData(value);

            if (_validation.IsValidLaisvosVietosParameters(postData))
            {
                if (_parkingBotDbContext != null)
                {
                    var data = postData["text"].Trim().Split('+');

                    var location = data[0];
                    var spot = data[1];
                    var dateFrom = DateTime.ParseExact(data[3], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    var dateTo = DateTime.ParseExact(data[5], "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    var username = postData["user_name"];
                    var action = postData["command"].Replace("%2F", string.Empty).Replace("/", string.Empty);

                    var availabilityRow = new Availability
                    {
                        Id = _generator.UniqueAvailabilityId(location, spot, dateFrom, dateTo),
                        Location = location,
                        Spot = spot,
                        DateFrom = dateFrom,
                        DateTo = dateTo
                    };

                    var logsRow = new Logs
                    {
                        Id = _generator.UniqueLogsId(username, location, spot, dateFrom, dateTo, action),
                        DateTime = DateTime.Now,
                        Action = action,
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        Location = location,
                        Spot = spot,
                        UserName = username
                    };

                    if (!_validation.AvailabilityRowExists(_parkingBotDbContext.Availability.ToList(), availabilityRow))
                    {
                        _parkingBotDbContext.Add(availabilityRow);
                        _parkingBotDbContext.SaveChanges();
                    }

                    if (!_validation.LogsRowExists(_parkingBotDbContext.Logs.ToList(), logsRow))
                    {
                        _parkingBotDbContext.Add(logsRow);
                        _parkingBotDbContext.SaveChanges();
                    }

                    var karmaPoints = _parkingBotDbContext.Logs.Where(x => x.UserName == username && x.Action == action).ToList().Count * 1000;

                    return Json(new Response
                    {
                        ResponseType = "in_channel",
                        Text = "```Laisva vieta pridėta! (" + location + " " + spot + " nuo " + dateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " iki " + dateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ")```",
                        Attachments = new List<Attachment>
                        {
                            new Attachment
                            {
                                Text = "+1000 karmos taškų. Jau turi +" + karmaPoints + " karmos taškų."
                            }
                        }
                    });
                }
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