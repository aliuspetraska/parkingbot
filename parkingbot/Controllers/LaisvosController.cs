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

        [HttpPost]
        public JsonResult Post()
        {
            var postData = _validation.ParsePostData(new StreamReader(Request.Body).ReadToEnd());

            Console.WriteLine(new StreamReader(Request.Body).ReadToEnd());

            if (_validation.IsValidLaisvosVietosParameters(postData))
            {
                Console.WriteLine("DATA VALID!");

                if (_parkingBotDbContext != null)
                {
                    Console.WriteLine("DB CONNECTION OK!");

                    var availability = _parkingBotDbContext.Availability.ToList();

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
    }
}