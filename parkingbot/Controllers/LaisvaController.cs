using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using parkingbot.Models;

namespace parkingbot.Controllers
{
    [Route("api/[controller]")]
    public class LaisvaController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "Works!";
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