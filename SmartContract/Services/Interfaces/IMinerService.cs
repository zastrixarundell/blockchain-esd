using System.Net.WebSockets;

namespace SmartContract.Services.Interfaces;

public interface IMinerService
{
    public Guid GenerateRandomUuid();
}