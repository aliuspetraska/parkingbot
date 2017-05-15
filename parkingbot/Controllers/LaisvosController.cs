using System.Collections.Generic;
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
        public LaisvosController()
        {
            _validation = new ValidationService();
            _generator = new GeneratorService();
        }

        private const string Dummy = "token=bKxl6WsWmBYMd8fMiHWkgcQ5&team_id=T0CR5GH16&team_domain=centriukas&channel_id=C569MHBFB&channel_name=parkingbotdevelopment&user_id=U4890CUGM&user_name=alius.petraska&command=%2Fkarma&text=seimyniskiu+9+nuo+2017-05-12+iki+2017-09-23&response_url=https%3A%2F%2Fhooks.slack.com%2Fcommands%2FT0CR5GH16%2F182696705778%2FxvMcZgcxGGk3JoBch1ZZhFQg";

        [HttpGet]
        public JsonResult Get()
        {
            var postData = _validation.ParsePostData(Dummy);

            if (_validation.IsValidLaisvosVietosParameters(postData))
            {
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
        public JsonResult Post([FromBody] string value)
        {
            var testObject = new Availability();

            return Json(testObject);
        }
    }
}