using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using SmartContract.Channels.Bases;
using SmartContract.Services;
using SmartContract.Services.Interfaces;

namespace SmartContract.Channels
{
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

                try {
                    Leave(miner);
                    miner.Socket.Dispose();
                } catch {

                }
            }
        }

        #region Channel-specific events

        protected override void Join(Miner miner, JsonObject information)
        {
            if (GetConnectedMiners().Contains(miner))
            {
                var data = new JsonObject
                {
                    { "message", "You can not double join!" }
                };

                // You can't double connect bubs!
                SendMessageToSocket(
                    miner.Socket,
                    GenerateChannelMessage("miner", "join:error", data)
                ).Wait();
                return;
            }

            miner.UUID = _service.GenerateRandomUuid();

            SendMessageToSocket(
                miner.Socket,
                GenerateChannelMessage(
                    "miner",
                    "join:success",
                    new JsonObject { { "uuid", miner.UUID.ToString() } }
                )
            ).Wait();
        }

        protected override void Leave(Miner miner)
        {
            string leaveReason = "normal";

            if(GetConnectedMiners().Contains(miner)) {
                _clients.Remove(miner);
                leaveReason = "hasty";
            }

            JsonObject jsonObject = new JsonObject
            {
                { "reason", leaveReason }
            };

            SendMessageToSocket(
                miner.Socket,
                GenerateChannelMessage("miner", "leave:success", jsonObject)
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
                        "job:reward",
                        new JsonObject
                        {
                            { "reward", reward }
                        })
                ).Wait();

                Broadcast("miner", "blockchain:append", new JsonObject
                {
                    { "user", user.Id },
                    { "miner", toBeRewarded.Miner.UUID },
                    { "reward", reward },
                    { "timestamp", DateTime.UtcNow }
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
                "job:new",
                new JsonObject
                {
                    { "request", user.Data },
                    { "user", user.Id }
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
            var miner = new Miner { Socket = socket };

            // Socket is connected to the server. Not on the channel specifically.
            _clients.Add(miner);

            var buffer = new byte[4096];

            WebSocketReceiveResult receiveResult = null;

            if(socket.State == WebSocketState.Open)
                receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (socket.State == WebSocketState.Open)
            {
                var str = System.Text.Encoding.Default.GetString(buffer, 0, receiveResult.Count);

                if (str == "")
                {
                    // This is most likely due to a disconnect but alas.
                    receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    continue;
                }

                // This means that this should be an some form of JSON object hopefully

                JsonObject? jsonObject = (JsonObject)JsonObject.Parse(str);

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
                    case "job:result":
                        AcceptResult(miner, jsonObject["data"].AsObject());
                        break;
                    default:
                        Console.WriteLine("Totally different event...");
                        break;
                }

                try
                {
                    receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                catch (WebSocketException)
                {
                    // The socket disconnected, need to clear the data
                    Console.WriteLine("The socket disconnected!");
                    _clients.Remove(miner);
                    await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Disconnected", CancellationToken.None);
                }
            }

            if(receiveResult.CloseStatus != null)
                await socket.CloseAsync(
                    receiveResult.CloseStatus.Value,
                    receiveResult.CloseStatusDescription,
                    CancellationToken.None);

            _clients.Remove(miner);
        }
    }
}