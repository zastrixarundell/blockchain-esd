using System.Collections.Generic;

namespace Miner.Services
{
    public interface IBlockchainService
    {
        IEnumerable<Blockchain> GetBlockchain();

        string BlockChainAsString();

        void AppendToBlockchain(Blockchain blockchain);
    }
}