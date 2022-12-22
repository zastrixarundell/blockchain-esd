using System.Net.WebSockets;
using SmartContract.Services.Interfaces;

namespace SmartContract.Services;

public class MinerService : IMinerService
{
    public Miner CreateMiner(WebSocket socket)
    {
        return new Miner { Socket = socket, UUID = GenerateRandomUuid() };
    }
    
    public Guid GenerateRandomUuid()
    {
        throw new NotImplementedException();
    }
}