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
    public class LogsController : Controller
    {
        private readonly ParkingBotDbContext _parkingBotDbContext;
        private readonly GeneratorService _generator;
        private readonly ValidationService _validation;

        public LogsController(ParkingBotDbContext parkingBotDbContext = null)
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
                var logItems = _parkingBotDbContext.Logs.OrderByDescending(o => o.DateTime).ToList();

                if (logItems.Count > 0)
                {
                    var rows = new List<Row>
                    {
                        new Row
                        {
                            Column = new List<string>
                            {
                                "KADA",
                                "VEIKSMAS",
                                "USERIS",
                                "IVAZIAVIMAS",
                                "VIETA",
                                "NUO",
                                "IKI"
                            }
                        }
                    };
                    
                    rows.AddRange(logItems.Select(item => new Row
                    {
                        Column = new List<string>
                        {
                            item.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                            item.Action.ToLower(),
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