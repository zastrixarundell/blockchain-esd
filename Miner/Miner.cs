using System;
using System.Collections;
using System.Collections.Generic;
using Miner.Services;
using Miner.Services.Implementations;

namespace Miner
{
    public class Miner
    {
        private IBlockchainService _blockchain = new BlockchainService();

        private readonly MinerSocket _socket;

        public Miner()
        {
            _socket = new SmartContractSocket(this);
        }
        
        public Guid? Uuid { get; set; }

        public void Register()
        {
            _socket.Register();
        }
    }
}