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
        Guid guid;

        do
        {
            guid = Guid.NewGuid();
        } while (Manager.MinerChannel.GetConnectedSockets().Any(m => m.UUID == guid));

        return guid;
    }
}