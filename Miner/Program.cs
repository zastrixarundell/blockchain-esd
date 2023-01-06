// See https://aka.ms/new-console-template for more information
using System;

namespace Miner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Miner miner = new Miner();
            
            // miner.connect();
            // miner.load();

            Blockchain blockchain = new Blockchain
            {
                UserId = "test_user_id",
                MinerId = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Reward = 10
            };
            
            miner.AppendToBlockchain(blockchain);

            Console.WriteLine(miner.BlockChainAsString());
        }
    }
}