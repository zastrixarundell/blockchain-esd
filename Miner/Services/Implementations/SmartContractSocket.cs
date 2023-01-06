using System;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Miner.Services.Implementations
{
    public class SmartContractSocket : MinerSocket
    {
        private WebsocketClient? _client;

        private static readonly ManualResetEvent ExitEvent = new ManualResetEvent(false);

        private readonly Uri url = new Uri("ws://localhost:5067/miners/connect/ws");

        public SmartContractSocket(Miner miner) : base(miner)
        {
        }

        #region handlers for specific events

            private void HandleJoin(string joinType, JsonObject data)
            {
                if (joinType == "success")
                {
                    Miner.Uuid = Guid.Parse(data["uuid"].ToString());
                    Console.WriteLine($"Registered miner as: {Miner.Uuid}");
                    return;
                }
                
                Console.WriteLine(data["message"].ToString());
                _client.Stop(WebSocketCloseStatus.InvalidPayloadData, "It happens.");
            }

            private void HandleLeave(string leaveType, JsonObject data)
            {
                if (leaveType == "success")
                {
                    _client.Stop(WebSocketCloseStatus.NormalClosure, "And stopping normally!");
                    ExitEvent.Set();
                }
                
                Console.WriteLine("An error happened while leaving!");
            }

            private void HandleBlokchain(string blockchainType, JsonObject data)
            {
                if (blockchainType == "append")
                {
                    Blockchain blockchain = new Blockchain
                    {
                        Reward = Convert.ToSingle(data["reward"].ToString()),
                        MinerId = Guid.Parse(data["miner"].ToString()),
                        UserId = data["user"].ToString(),
                        Timestamp = DateTime.Parse(data["timestamp"].ToString())
                    };
                    
                    Miner.AppendToBlockchain(blockchain);
                    
                    Console.WriteLine("Added to blockchain!");
                }
            }

            private void HandleJob(string jobEventType, JsonObject data)
            {
                switch (jobEventType)
                {
                    case "reward":
                        float reward = Convert.ToSingle(data["reward"].ToString());
                        Miner.Balance += reward;
                        Console.WriteLine($"I got a new reward of: {reward}!");
                        break;
                    case "new":
                        string job = data["request"].ToString();
                        string user = data["user"].ToString(); 
                        // Handle the new job
                        Console.WriteLine($"Got new job from {user} with the value of: {job}");
                        JobRunner runner = new JobRunner(job);
                        string result = runner.CalculateHash().Result;
                        Console.WriteLine($"Calculated job hash: {result}!");
                        
                        JsonObject jsonObject = new JsonObject
                        {
                            {"topic", "miner"},
                            {"event", "job:result"},
                            {
                                "data", 
                                new JsonObject
                                {
                                    { "data", job },
                                    { "user", user },
                                    { "result", result }
                                }
                            }
                        };
                    
                        _client.SendInstant(jsonObject.ToJsonString()).Wait();
                        
                        break;
                }
            }

        #endregion

        #region general methods

            private void StartUI()
            {
                Task.Run(() => {
                    while(_client.IsRunning) {
                        Console.WriteLine("Super cool miner UI:\n");
                        Console.WriteLine("M - Check your info!");
                        Console.WriteLine("B - Blockchain status!");
                        Console.WriteLine("X - Stop the miner!\n");
                        Console.Write("Pick your poison: ");

                        string option = Console.ReadLine();

                        switch (option)
                        {
                            case "M":
                                Console.WriteLine(Miner);
                                break;
                            case "B":
                                Console.WriteLine(Miner.CurrentBlockchain());
                                break;
                            case "X":
                                var jsonObject = new JsonObject
                                {
                                    {"topic", "miner"},
                                    {"event", "leave"},
                                };
                    
                                _client.SendInstant(jsonObject.ToJsonString()).Wait();
                                break;
                            default:
                                Console.WriteLine($"\"{option}\" is not an option!");
                                break;
                        }
                        
                        Thread.Sleep(1000);
                        
                        Console.Write("\n\n\n");
                    }
                });
            }

        #endregion

        #region handlers for running logic

            public void HandleMessage(ResponseMessage message)
            {
                Console.WriteLine($"Got a new message: {message}");
                
                JsonObject? jsonObject = (JsonObject)JsonObject.Parse(message.ToString());

                string[] contractEvent = jsonObject["event"].ToString().Split(":");

                JsonObject data = (JsonObject) jsonObject["data"];

                switch (contractEvent[0])
                {
                    case "join":
                        HandleJoin(contractEvent[1], data);
                        break;
                    case "leave":
                        HandleLeave(contractEvent[1], data);
                        break;
                    case "broadcast":
                        Console.WriteLine($"Got broadcast: \"{jsonObject["data"]["message"]}\" from source: \"{contractEvent[1]}\"");
                        break;
                    case "blockchain":
                        HandleBlokchain(contractEvent[1], data);
                        break;
                    case "job":
                        HandleJob(contractEvent[1], data);
                        break;
                }
            }

            public override void Register()
            {
                using (_client = new WebsocketClient(url))
                {
                    // Setting up the client
                    
                    _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                    
                    _client.MessageReceived.Subscribe(HandleMessage);
                    
                    _client.Start().Wait();
                    
                    // Joined on the event!
                    
                    JsonObject jsonObject = new JsonObject
                    {
                        {"topic", "miner"},
                        {"event", "join"}
                    };
                    
                    _client.SendInstant(jsonObject.ToJsonString()).Wait();
                    
                    Console.WriteLine("Hey, I started!");
                    
                    // Client is now registered

                    StartUI();

                    ExitEvent.WaitOne();
                }
            }

        #endregion
        
    }
}