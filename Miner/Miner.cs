using System.Collections.Generic;

namespace Miner
{
    public class Miner
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
    }
}