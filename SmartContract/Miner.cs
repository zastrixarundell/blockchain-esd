using System;
using System.ComponentModel.DataAnnotations;
using System.Net.WebSockets;

namespace SmartContract
{
    public class Miner
    {
        public Guid? UUID { get; set; }
        [Required]
        public WebSocket Socket { get; set; }
    }
}