using System;
using System.ComponentModel.DataAnnotations;

namespace parkingbot.Models
{
    public class Logs
    {
        [Required, Key]
        public string Id { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        [Required, StringLength(255)]
        public string UserName { get; set; }

        [Required, StringLength(255)]
        public string Action { get; set; }

        [Required, StringLength(255)]
        public string Location { get; set; }

        [Required, StringLength(255)]
        public string Spot { get; set; }

        [Required]
        public DateTime DateFrom { get; set; }

        [Required]
        public DateTime DateTo { get; set; }
    }
}