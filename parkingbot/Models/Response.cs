using System.Collections.Generic;
using Newtonsoft.Json;

namespace parkingbot.Models
{
    public class Attachment
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class Response
    {
        [JsonProperty("response_type")]
        public string ResponseType { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("attachments")]
        public List<Attachment> Attachments { get; set; }
    }
}