using System.Net.WebSockets;
using SmartContract.Channels.Interfaces;

namespace SmartContract.Channels;

public class MinerChannel : IMinerChannel
{
    // This logic for channels is, by idea at least, based off Elixir/Phoenix's channel mechanic for websockets.
    
    private IEnumerable<WebSocket> _clients = new List<WebSocket>();

    public void Dispose()
    {
        foreach (var client in _clients)  
            client.Dispose();
    }

    // Driving logic    

    public async Task Listen(WebSocket socket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await socket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            await socket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);
        
            receiveResult = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }
        
        await socket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
    
    // All of the cool stuff

    public IEnumerable<WebSocket> GetConnectedSockets()
    {
        return _clients;
    }
}