using System.Net.WebSockets;

namespace SmartContract.Services.Interfaces;

public interface IMinerService
{
    public Miner CreateMiner(WebSocket socket);
    public Guid GenerateRandomUuid();
}