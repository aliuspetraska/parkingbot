using System.Collections.Generic;

namespace parkingbot.Models
{
    public class Attachment
    {
        public string text { get; set; }
    }

    public class Response
    {
        public string response_type { get; set; }
        public string text { get; set; }
        public List<Attachment> attachments { get; set; }
    }
}