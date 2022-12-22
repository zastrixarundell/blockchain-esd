using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;

namespace SmartContract.Channels.Bases;

public abstract class ChannelBase
{
    protected abstract void Dispose();

    public abstract IEnumerable<WebSocket> GetConnectedSockets();

    public abstract Task Listen(WebSocket socket);

    public abstract void Broadcast(string topic, string channelName, JsonObject data);
    
    protected abstract void Leave(WebSocket socket, String? leaveReason = null);

    protected abstract void Join(WebSocket socket, JsonObject information);
    
    /**
     * Listen to the socket and get a WebSocketReceiveResult. This will be decoded to a
     * string and JSON object at a later point.
     */
    protected async Task<WebSocketReceiveResult> ListenToSocket(WebSocket socket, int bufferSize = 4096)
    {
        var buffer = new byte[bufferSize];
        return await socket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
    }

    /**
     * Sends a messages to the connected web socket.
     */
    protected async Task SendMessageToSocket(WebSocket socket, JsonObject message)
    {
        var encoded = Encoding.UTF8.GetBytes(message.ToJsonString());
        var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
        await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }
    
    protected async Task CloseConnectionEvent(WebSocket socket, string topic, JsonObject? data)
    {
        data ??= new JsonObject();

        await SendMessageToSocket(
            socket, GenerateChannelMessage(topic, "disconnect", data));
    }

    protected JsonObject GenerateChannelMessage(string topic, string eventName, JsonObject? data)
    {
        data ??= new JsonObject();
        
        return new JsonObject
        {
            { "topic", topic },
            { "event", eventName },
            { "data", data }
        };
    }
}