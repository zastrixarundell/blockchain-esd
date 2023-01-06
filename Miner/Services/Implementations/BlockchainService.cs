using System.Collections;
using System.Collections.Generic;

namespace Miner.Services.Implementations
{
    public class BlockchainService : IBlockchainService
    {
        private readonly List<Blockchain> _blockchain = new List<Blockchain>();

        public void AppendToBlockchain(Blockchain blockchain)
        {
            _blockchain.Add(blockchain);
        }

        public IEnumerable<Blockchain> GetBlockchain()
        {
            return _blockchain;
        }

        public string BlockChainAsString()
        {
            var lines = new ArrayList { $"| {"Miner UUID",-36} | {"User",-20} | {"Reward", -8} | Timestamp" };

            foreach (var blockchain in _blockchain)
                lines.Add(blockchain.ToString());

            return string.Join("\n", lines.ToArray());
        }
    }
}