using System.Net.WebSockets;

namespace SmartContract.Channels.Interfaces;

public interface IMinerChannel : IDisposable
{
    public IEnumerable<WebSocket> GetConnectedSockets();
    public Task Listen(WebSocket socket);
}