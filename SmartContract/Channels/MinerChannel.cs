using System.Net.WebSockets;
using System.Text.Json.Nodes;
using SmartContract.Channels.Bases;

namespace SmartContract.Channels;

public class MinerChannel : ChannelBase
{
    // This logic for channels is, by idea at least, based off Elixir/Phoenix's channel mechanic for websockets.

    private readonly List<WebSocket> _clients = new();
    private readonly List<WebSocket> _connectedClients = new();

    protected override void Dispose()
    {
        foreach (var client in _clients)
        {
            Leave(client);
            client.Dispose();
        }
    }

    #region Channel-specific events

        protected override void Join(WebSocket socket, JsonObject information)
        {
            if (_connectedClients.Contains(socket))
            {
                var data = new JsonObject
                {
                    { "message", "You can not double join" }
                };
                    
                // You can't double connect bubs!
                SendMessageToSocket(
                    socket,
                    GenerateChannelMessage("miner", "error:join", data)
                ).Wait();
                return;
            }
            
            _connectedClients.Add(socket);

            SendMessageToSocket(
                socket,
                GenerateChannelMessage("miner", "success:join", null)
            ).Wait();
        }
        
        protected override void Leave(WebSocket socket, string? leaveReason = null)
        {
            if (!_connectedClients.Contains(socket))
            {
                var data = new JsonObject
                {
                    // lol
                    { "message", "You need to be logged in to log out. Please log in to log out." }
                };
                
                SendMessageToSocket(
                    socket,
                    GenerateChannelMessage("miner", "error:leave", data)
                ).Wait();
                return;
            }
        
            _connectedClients.Remove(socket);

            JsonObject jsonObject = new JsonObject
            {
                { "reason", leaveReason == null ? "normal" : "hasty" }
            };

            SendMessageToSocket(
                socket,
                GenerateChannelMessage("miner", "success:leave", jsonObject)
            ).Wait();
        }

        public override void Broadcast(string topic, string eventName, JsonObject data)
        {
            foreach (var socket in _connectedClients)
            {
                SendMessageToSocket(
                    socket,
                    GenerateChannelMessage(topic, eventName, data)
                ).Wait();
            }
        }
        
    #endregion
    
    // All of the cool stuff

    public override IEnumerable<WebSocket> GetConnectedSockets()
    {
        return _clients;
    }

    // Driving logic for the connection / black magic
    public override async Task Listen(WebSocket socket)
    {
        // Socket is connected to the server. Not on the channel specifically.
        _clients.Add(socket);
        
        var buffer = new byte[4096];
        var receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        
        while (!receiveResult.CloseStatus.HasValue)
        {
            var str = System.Text.Encoding.Default.GetString(buffer, 0, receiveResult.Count);

            if (str == "")
            {
                // This is most likely due to a disconnect but alas.
                receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                continue;
            }
            
            // This means that this should be an some form of JSON object hopefully

            JsonObject? jsonObject = (JsonObject) JsonObject.Parse(str);

            if (jsonObject == null)
            {
                receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                continue;
            }

            Console.WriteLine(jsonObject?.ToJsonString());
            
            if (jsonObject == null)
            {
                receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                continue;
            }

            switch (jsonObject["event"].ToString())
            {
                case "join":
                    Join(socket, jsonObject);
                    break;
                case "leave":
                    Leave(socket);
                    break;
                default:
                    Console.WriteLine("Totally different event...");
                    break;
            }

            receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await socket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);

        _clients.Remove(socket);
        _connectedClients.Remove(socket);
    }
}