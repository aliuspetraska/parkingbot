using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using parkingbot.Models;

namespace parkingbot.Controllers
{
    [Route("api/[controller]")]
    public class KarmaController : Controller
    {
        [HttpGet]
        public JsonResult Get()
        {
            var result = new Response
            {
                response_type = "ephemeral",
                text = "text",
                attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        text = "attachment"
                    }
                }
            };

            return Json(result);
        }

        [HttpPost]
        public JsonResult Post([FromBody] string value)
        {
            var result = new Response
            {
                response_type = "ephemeral",
                text = "text",
                attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        text = "attachment"
                    }
                }
            };

            return Json(result);
        }
    }
}