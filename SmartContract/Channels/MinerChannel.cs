using System.Net.WebSockets;
using System.Text.Json.Nodes;
using SmartContract.Channels.Bases;
using SmartContract.Services;
using SmartContract.Services.Interfaces;

namespace SmartContract.Channels;

public class MinerChannel : ChannelBase
{
    // This logic for channels is, by idea at least, based off Elixir/Phoenix's channel mechanic for websockets.

    private readonly IMinerService _service = new MinerService();
    
    private readonly List<Miner> _clients = new();

    private readonly List<Calculation> _calculations = new();

    protected override void Dispose()
    {
        foreach (var miner in _clients)
        {
            Leave(miner);
            miner.Socket.Dispose();
        }
    }

    #region Channel-specific events

        protected override void Join(Miner miner, JsonObject information)
        {
            if (GetConnectedMiners().Contains(miner))
            {
                var data = new JsonObject
                {
                    { "message", "You can not double join" }
                };
                    
                // You can't double connect bubs!
                SendMessageToSocket(
                    miner.Socket,
                    GenerateChannelMessage("miner", "error:join", data)
                ).Wait();
                return;
            }

            miner.UUID = _service.GenerateRandomUuid();

            SendMessageToSocket(
                miner.Socket,
                GenerateChannelMessage(
                    "miner",
                    "success:join",
                    new JsonObject { {"uuid", miner.UUID.ToString() }}
                )
            ).Wait();
        }
        
        protected override void Leave(Miner miner, string? leaveReason = null)
        {
            if (!GetConnectedMiners().Contains(miner))
            {
                var data = new JsonObject
                {
                    // lol
                    { "message", "You need to be logged in to log out. Please log in to log out." }
                };
                
                SendMessageToSocket(
                    miner.Socket,
                    GenerateChannelMessage("miner", "error:leave", data)
                ).Wait();
                return;
            }
        
            _clients.Remove(miner);

            JsonObject jsonObject = new JsonObject
            {
                { "reason", leaveReason == null ? "normal" : "hasty" }
            };

            SendMessageToSocket(
                miner.Socket,
                GenerateChannelMessage("miner", "success:leave", jsonObject)
            ).Wait();
        }
        
        protected override void AcceptResult(Miner miner, JsonObject data)
        {
            var request = data["data"].ToString();
            if (String.IsNullOrEmpty(request))
                return;

            var userMatch = Manager.UserService.GetAll().Count(user => user.Id == data["user"].ToString());

            if (userMatch == 0)
                return;

            var user = Manager.UserService.GetAll().First(user => user.Id == data["user"].ToString());
            
            var result = data["result"].ToString();
            if (String.IsNullOrEmpty(result))
                return;
            
            var calculation = new Calculation
            {
                Miner = miner,
                Data = request,
                Requester = user,
                Result = result
            };

            _calculations.Add(calculation);
            
            Console.WriteLine("Accepted calculation: " + calculation);
            Console.WriteLine("Is the calculation correct: " + calculation.Valid());

            var requestsCalculations =
                _calculations.Where(calc => calc.Requester.Id == user.Id).ToList();

            // Check if this is the last queued miner
            if (requestsCalculations.Count != _clients.Count)
                return;

            // This means it was the last miner for the connection
            
            var validOnes = requestsCalculations.Where(calc => calc.Valid()).ToList();

            foreach (var toBeRewarded in validOnes)
            {
                var reward = 1.0 / validOnes.Count;

                SendMessageToSocket(
                    toBeRewarded.Miner.Socket,
                    GenerateChannelMessage(
                        "miner",
                        "reward",
                        new JsonObject
                        {
                            { "reward", reward }
                        })
                ).Wait();
                
                Broadcast("miner", "blockchain_update", new JsonObject
                {
                    { "request", user.Id },
                    { "miner", toBeRewarded.Miner.UUID },
                    { "reward", reward }
                });
            }
            
            // remove the request from memory
            _calculations.RemoveAll(calc => calc.Requester.Id == user.Id);
            Manager.UserService.RemoveFromQueue(user);
            
            // queue new one
            var users = Manager.UserService.GetAll().ToList();

            if (!users.Any())
                return;

            user = users.First();

            Manager.MinerChannel.Broadcast(
                "miner",
                "new_job",
                new JsonObject
                {
                    { "request", user.Data },
                    { "sender", user.Id }
                });
        }

        public override void Broadcast(string topic, string eventName, JsonObject data)
        {
            foreach (var miner in GetConnectedMiners())
            {
                SendMessageToSocket(
                    miner.Socket,
                    GenerateChannelMessage(topic, eventName, data)
                ).Wait();
            }
        }
        
    #endregion
    
    // All of the cool stuff

    public override IEnumerable<Miner> GetConnectedMiners()
    {
        return _clients.Where(m => m.UUID != null);
    }

    // Driving logic for the connection / black magic
    public override async Task Listen(WebSocket socket)
    {
        var miner = new Miner{ Socket = socket };
        
        // Socket is connected to the server. Not on the channel specifically.
        _clients.Add(miner);
        
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
                    Join(miner, jsonObject);
                    break;
                case "leave":
                    Leave(miner);
                    break;
                case "result":
                    AcceptResult(miner, jsonObject["data"].AsObject());
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

        _clients.Remove(miner);
    }
}