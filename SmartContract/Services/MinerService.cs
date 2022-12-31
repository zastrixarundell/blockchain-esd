using System;
using System.Linq;
using System.Net.WebSockets;
using SmartContract.Services.Interfaces;

namespace SmartContract.Services
{
    public class MinerService : IMinerService
    {
        public Guid GenerateRandomUuid()
        {
            Guid guid;

            do
            {
                guid = Guid.NewGuid();
            } while (Manager.MinerChannel.GetConnectedMiners().Any(m => m.UUID == guid));

            return guid;
        }
    }
}