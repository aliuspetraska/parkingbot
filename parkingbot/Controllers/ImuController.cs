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

        [HttpGet]
        public JsonResult Get()
        {
            var dummy = "token=tgOdv9MbvpiCedX8V4oLYYJL&team_id=T0CR5GH16&team_domain=centriukas&channel_id=D5DGE5EHH&channel_name=directmessage&user_id=U4890CUGM&user_name=alius.petraska&command=%2Fimu&text=juozapaviciaus+3+nuo+2017-05-16+iki+2017-05-17&response_url=https%3A%2F%2Fhooks.slack.com%2Fcommands%2FT0CR5GH16%2F183504222305%2FXLBcttAbLoOrPQ1LiKnMR25x";

            var postData = _validation.ParsePostData(dummy);

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
                            // magija happens here
                        }

                        // insert into logs

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

        [HttpPost]
        public void Post()
        {
            var postData = _validation.ParsePostData(new StreamReader(Request.Body).ReadToEnd());
        }
    }
}