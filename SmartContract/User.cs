using System;
using System.ComponentModel.DataAnnotations;

namespace SmartContract
{
    public class User
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
        [Required]
        public string Data { get; set; }
    }
}
