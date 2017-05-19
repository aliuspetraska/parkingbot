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
    public class LaisvosController : Controller
    {
        private static ValidationService _validation;
        private static GeneratorService _generator;
        private static ParkingBotDbContext _parkingBotDbContext;

        public LaisvosController(ParkingBotDbContext parkingBotDbContext = null)
        {
            _validation = new ValidationService();
            _generator = new GeneratorService();
            _parkingBotDbContext = parkingBotDbContext;
        }

        [HttpPost]
        public JsonResult Post()
        {
            var postData = _validation.ParsePostData(new StreamReader(Request.Body).ReadToEnd());

            if (_parkingBotDbContext != null)
            {
                var availability = FilterOutAvailability(_parkingBotDbContext.Availability.ToList().OrderBy(o => o.DateFrom).ToList());

                if (availability.Count > 0)
                {
                    return Json(new Response
                    {
                        ResponseType = "ephemeral",
                        Text = "```" + _generator.GenerateAvailabilityTable(availability) + "```",
                        Attachments = new List<Attachment>
                        {
                            new Attachment
                            {
                                Text = "/imu " + availability[0].Location + " " + availability[0].Spot + " nuo " + availability[0].DateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " iki " + availability[0].DateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                            }
                        }
                    });
                }

                return Json(new Response
                {
                    ResponseType = "ephemeral",
                    Text = "```Laisvų vietų nėra.```",
                    Attachments = new List<Attachment>
                    {
                        new Attachment
                        {
                            Text = _generator.ReturnWhatYouTyped(postData)
                        }
                    }
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

        private static List<Availability> FilterOutAvailability(IEnumerable<Availability> availabilities)
        {
            var todayString = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var today = DateTime.ParseExact(todayString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var filtered = new List<Availability>();

            foreach (var item in availabilities)
            {
                if (item.DateTo >= today)
                {
                    filtered.Add(item);
                }
                else
                {
                    _parkingBotDbContext.Availability.Remove(item);
                    _parkingBotDbContext.SaveChanges();
                }
            }

            var result = new List<Availability>();

            foreach (var item in filtered)
            {
                if (item.DateFrom < today)
                {
                    var updateRow = new Availability
                    {
                        Id = _generator.UniqueAvailabilityId(item.Location, item.Spot, today, item.DateTo),
                        Location = item.Location,
                        Spot = item.Spot,
                        DateFrom = today,
                        DateTo = item.DateTo
                    };

                    _parkingBotDbContext.Availability.Remove(item);
                    _parkingBotDbContext.SaveChanges();

                    if (!_validation.AvailabilityRowExists(_parkingBotDbContext.Availability.ToList(), updateRow))
                    {
                        _parkingBotDbContext.Availability.Add(updateRow);
                        _parkingBotDbContext.SaveChanges();
                    }

                    item.Id = updateRow.Id;
                    item.DateFrom = updateRow.DateFrom;
                }

                result.Add(item);
            }

            return result;
        }
    }
}