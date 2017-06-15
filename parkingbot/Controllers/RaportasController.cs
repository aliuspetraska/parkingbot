using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using parkingbot.Models;
using parkingbot.Services;

namespace parkingbot.Controllers
{
    [Route("slack/[controller]")]
    public class RaportasController : Controller
    {
        private readonly ParkingBotDbContext _parkingBotDbContext;
        private readonly GeneratorService _generator;
        private readonly ValidationService _validation;

        public RaportasController(ParkingBotDbContext parkingBotDbContext = null)
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
                var paimtos = _parkingBotDbContext.Logs.Where(x => x.Action == "imu").ToList();
                var laisvos = _parkingBotDbContext.Logs.Where(x => x.Action == "laisva").ToList();

                var totalUtilization = TotalUtilization(paimtos, laisvos);
                
                var laisvosGroup = laisvos.GroupBy(g => g.DateFrom.Month).ToList();
                
                var rows = new List<Row>
                {
                    new Row
                    {
                        Column = new List<string>
                        {
                            "MENUO",
                            "UTILIZACIJA"
                        }
                    }
                };
                
                foreach (var menuo in laisvosGroup)
                {
                    var paimtosTaMenesi = paimtos.Where(x => x.DateFrom.Month == menuo.ToList()[0].DateFrom.Month);
                    
                    rows.Add(new Row
                    {
                        Column = new List<string>
                        {
                            menuo.ToList()[0].DateFrom.ToString("MMMM", CultureInfo.InvariantCulture).ToUpper(),
                            TotalUtilization(paimtosTaMenesi, menuo).ToString(CultureInfo.InvariantCulture) + "%"
                        }
                    });
                }
                
                rows.Add(new Row
                {
                    Column = new List<string>
                    {
                       "-------", "-------------"
                    }
                });
                
                rows.Add(new Row
                {
                    Column = new List<string>
                    {
                        "TOTAL", totalUtilization.ToString(CultureInfo.InvariantCulture) + "%"
                    }
                });
                
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

        private static double TotalUtilization(IEnumerable<Logs> paimtos, IEnumerable<Logs> laisvos)
        {
            return Math.Round(CountTotalDays(paimtos) * 100.0 / CountTotalDays(laisvos), 2);
        }
        
        private static int CountTotalDays(IEnumerable<Logs> items)
        {
            var totalDays = 0;

            foreach (var item in items)
            {
                totalDays += (item.DateTo - item.DateFrom).Days + 1 - WeekendsCount(item.DateFrom, item.DateTo);
            }

            return totalDays;
        }
        
        private static int WeekendsCount(DateTime dateFrom, DateTime dateTo)
        {
            var result = 0;

            var difference = (dateTo - dateFrom).Days;

            for (var i = 0; i < difference; i++)
            {
                var day = dateFrom.AddDays(i);

                if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
                {
                    result++;
                }
            }

            return result;
        }
    }
}