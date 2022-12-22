using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;

namespace SmartContract.Channels.Bases;

public abstract class ChannelBase
{
    protected abstract void Dispose();

    /**
     * Get a list of all of the connected miners.
     */
    public abstract IEnumerable<Miner> GetConnectedMiners();

    /**
     * Main consumer function which should be put in the controller.
     */
    public abstract Task Listen(WebSocket socket);

    /**
     * Broadcast a messages across all of the connected nodes.
     */
    public abstract void Broadcast(string topic, string channelName, JsonObject data);
    
    /**
     * Logic which needs to be ran when the clients wants to disconnect from the channel.
     * This does not close the websocket connection, it should just remove the websocket
     * from the current state of the server.
     */
    protected abstract void Leave(Miner miner, String? leaveReason = null);

    /**
     * Logic which needs to be ran on the channel when the client joins.
     */
    protected abstract void Join(Miner miner, JsonObject information);
    
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

    /**
     * Generate a message which can be consumed by the websocket client.
     */
    protected JsonObject GenerateChannelMessage(string topic, string eventName, JsonObject? data)
    {
        data ??= new JsonObject();
        
        return new JsonObject
        {
            { "topic", topic },
            { "event", eventName },
            // For some reason it would crash if this wasn't written here
            { "data", (JsonObject?) JsonNode.Parse(data.ToJsonString()) }
        };
    }
}