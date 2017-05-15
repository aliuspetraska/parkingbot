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
    [Route("api/[controller]")]
    public class LaisvosController : Controller
    {
        private readonly ValidationService _validation;
        private readonly GeneratorService _generator;
        private readonly ParkingBotDbContext _parkingBotDbContext;

        public LaisvosController(ParkingBotDbContext parkingBotDbContext = null)
        {
            _validation = new ValidationService();
            _generator = new GeneratorService();
            _parkingBotDbContext = parkingBotDbContext;
        }

        [HttpGet]
        public JsonResult Get()
        {
            var dummy = "token=tgOdv9MbvpiCedX8V4oLYYJL&team_id=T0CR5GH16&team_domain=centriukas&channel_id=D5DGE5EHH&channel_name=directmessage&user_id=U4890CUGM&user_name=alius.petraska&command=%2Flaisvos&text=vietos&response_url=https%3A%2F%2Fhooks.slack.com%2Fcommands%2FT0CR5GH16%2F183844422884%2Fso5niezRBCZ7B5qvWN4dOFNA";

            var postData = _validation.ParsePostData(dummy);

            if (_validation.IsValidLaisvosVietosParameters(postData))
            {
                if (_parkingBotDbContext != null)
                {
                    var availability = FilterOutAvailability(_parkingBotDbContext.Availability.ToList().OrderBy(o => o.DateFrom).ToList());

                    if (availability.Count > 0)
                    {
                        return Json(new Response
                        {
                            ResponseType = "ephemeral",
                            Text = "```" + _generator.GenerateTable(availability) + "```",
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

        [HttpPost]
        public JsonResult Post()
        {
            var postData = _validation.ParsePostData(new StreamReader(Request.Body).ReadToEnd());

            if (_validation.IsValidLaisvosVietosParameters(postData))
            {
                if (_parkingBotDbContext != null)
                {
                    var availability = FilterOutAvailability(_parkingBotDbContext.Availability.ToList().OrderBy(o => o.DateFrom).ToList());

                    if (availability.Count > 0)
                    {
                        return Json(new Response
                        {
                            ResponseType = "ephemeral",
                            Text = "```" + _generator.GenerateTable(availability) + "```",
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

        private static List<Availability> FilterOutAvailability(List<Availability> availabilities)
        {
            var result = new List<Availability>();

            var todayString = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var today = DateTime.ParseExact(todayString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var filtered = availabilities.Where(item => item.DateTo > today).ToList();

            foreach (var item in filtered)
            {
                if (item.DateFrom < today)
                {
                    item.DateFrom = today;
                }

                result.Add(item);
            }

            return result;
        }
    }
}