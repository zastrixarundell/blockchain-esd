using System;

namespace Miner
{
    public class Blockchain
    {
        public String UserId { get; set; }
        
        public Guid MinerId { get; set; }
        
        public float Reward { get; set; }
        
        public DateTime Timestamp { get; set; }
    }
}