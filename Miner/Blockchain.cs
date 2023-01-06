using System;

namespace Miner
{
    public class Blockchain
    {
        public String UserId { get; set; }
        
        public Guid MinerId { get; set; }
        
        public float Reward { get; set; }
        
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"| {MinerId.ToString(),-36} | {UserId,-20} | {Reward:0.00000} | {Timestamp:MM/dd/yyyy HH:mm:ss zzz}";
        }
    }
}