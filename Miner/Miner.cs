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

        private readonly IMinerSocket _socket = new SmartContractSocket();
        
        public Guid? Uuid { get; set; }

        public void Register()
        {
            _socket.Register(this);
        }
    }
}