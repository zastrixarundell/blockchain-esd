using System.Collections.Generic;

namespace Miner.Services
{
    public interface IBlockchainService
    {
        IEnumerable<Blockchain> GetBlockchain();

        string BlockChainAsString();
    }
}