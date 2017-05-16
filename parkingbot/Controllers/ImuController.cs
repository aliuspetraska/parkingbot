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
    public class ImuController : Controller
    {
        private readonly ValidationService _validation;
        private readonly GeneratorService _generator;
        private readonly ParkingBotDbContext _parkingBotDbContext;

        public ImuController(ParkingBotDbContext parkingBotDbContext = null)
        {
            _validation = new ValidationService();
            _generator = new GeneratorService();
            _parkingBotDbContext = parkingBotDbContext;
        }

        [HttpPost]
        public JsonResult Post()
        {
            var postData = _validation.ParsePostData(new StreamReader(Request.Body).ReadToEnd());

            if (_validation.IsValidLaisvaImuParameters(postData))
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

                    var result =
                        _parkingBotDbContext.Availability.Where(
                            a => a.Location == location && a.Spot == spot && a.DateFrom <= dateFrom &&
                                 a.DateTo >= dateTo).ToList();

                    if (result.Count > 0)
                    {
                        if (result[0].DateFrom == dateFrom && result[0].DateTo == dateTo)
                        {
                            _parkingBotDbContext.Availability.Remove(result[0]);
                            _parkingBotDbContext.SaveChanges();
                        }
                        else
                        {
                            if (result[0].DateFrom == dateFrom && result[0].DateTo != dateTo)
                            {
                                var deleteRow = result[0];

                                _parkingBotDbContext.Availability.Remove(deleteRow);
                                _parkingBotDbContext.SaveChanges();

                                var updateRow = new Availability
                                {
                                    Id = _generator.UniqueAvailabilityId(result[0].Location, result[0].Spot, dateTo.AddDays(1), result[0].DateTo),
                                    Location = result[0].Location,
                                    Spot = result[0].Spot,
                                    DateFrom = dateTo.AddDays(1),
                                    DateTo = result[0].DateTo
                                };

                                if (!_validation.AvailabilityRowExists(_parkingBotDbContext.Availability.ToList(), updateRow))
                                {
                                    _parkingBotDbContext.Availability.Add(updateRow);
                                    _parkingBotDbContext.SaveChanges();
                                }
                            }
                            else if (result[0].DateFrom != dateFrom && result[0].DateTo == dateTo)
                            {
                                var deleteRow = result[0];

                                _parkingBotDbContext.Availability.Remove(deleteRow);
                                _parkingBotDbContext.SaveChanges();

                                var updateRow = new Availability
                                {
                                    Id = _generator.UniqueAvailabilityId(result[0].Location, result[0].Spot, result[0].DateFrom, dateFrom.AddDays(-1)),
                                    Location = result[0].Location,
                                    Spot = result[0].Spot,
                                    DateFrom = result[0].DateFrom,
                                    DateTo = dateFrom.AddDays(-1)
                                };

                                if (!_validation.AvailabilityRowExists(_parkingBotDbContext.Availability.ToList(), updateRow))
                                {
                                    _parkingBotDbContext.Availability.Add(updateRow);
                                    _parkingBotDbContext.SaveChanges();
                                }
                            }
                            else
                            {
                                var deleteRow = result[0];

                                _parkingBotDbContext.Availability.Remove(deleteRow);
                                _parkingBotDbContext.SaveChanges();

                                var updateRow1 = new Availability
                                {
                                    Id = _generator.UniqueAvailabilityId(result[0].Location, result[0].Spot, result[0].DateFrom, dateFrom.AddDays(-1)),
                                    Location = result[0].Location,
                                    Spot = result[0].Spot,
                                    DateFrom = result[0].DateFrom,
                                    DateTo = dateFrom.AddDays(-1)
                                };

                                if (!_validation.AvailabilityRowExists(_parkingBotDbContext.Availability.ToList(), updateRow1))
                                {
                                    _parkingBotDbContext.Availability.Add(updateRow1);
                                    _parkingBotDbContext.SaveChanges();
                                }

                                var updateRow2 = new Availability
                                {
                                    Id = _generator.UniqueAvailabilityId(result[0].Location, result[0].Spot, dateTo.AddDays(1), result[0].DateTo),
                                    Location = result[0].Location,
                                    Spot = result[0].Spot,
                                    DateFrom = dateTo.AddDays(1),
                                    DateTo = result[0].DateTo
                                };

                                if (!_validation.AvailabilityRowExists(_parkingBotDbContext.Availability.ToList(), updateRow2))
                                {
                                    _parkingBotDbContext.Availability.Add(updateRow2);
                                    _parkingBotDbContext.SaveChanges();
                                }
                            }
                        }

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

                        if (!_validation.LogsRowExists(_parkingBotDbContext.Logs.ToList(), logsRow))
                        {
                            _parkingBotDbContext.Add(logsRow);
                            _parkingBotDbContext.SaveChanges();
                        }

                        return Json(new Response
                        {
                            ResponseType = "ephemeral",
                            Text = "```Vieta tavo! (" + location + " " + spot + " nuo " +
                                   dateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " iki " +
                                   dateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ")```"
                        });
                    }

                    return Json(new Response
                    {
                        ResponseType = "ephemeral",
                        Text = "```Soriukas, nepaėjo. Tokios vietos nėra arba ją jau paėmė kolega.```",
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