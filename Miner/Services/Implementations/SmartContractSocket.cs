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

        #endregion

        #region handlers for running logic

            public void HandleMessage(ResponseMessage message)
            {
                JsonObject? jsonObject = (JsonObject)JsonObject.Parse(message.ToString());

                string[] contractEvent = jsonObject["event"].ToString().Split(":");

                JsonObject data = (JsonObject) jsonObject["data"];

                switch (contractEvent[0])
                {
                    case "join":
                        HandleJoin(contractEvent[1], data);
                        break;
                    case "broadcast":
                        Console.WriteLine($"Got broadcast: \"{jsonObject["data"]["message"]}\" from source: \"{contractEvent[1]}\"");
                        break;
                    case "blockchain":
                        
                        break;
                }
            }

            public override void Register()
            {
                using (_client = new WebsocketClient(url))
                {
                    // Setting up the client
                    
                    _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                    _client.ReconnectionHappened.Subscribe(info =>
                        Console.WriteLine($"Reconnection happened, type: {info.Type}"));
                    _client.DisconnectionHappened.Subscribe(info =>
                        Console.WriteLine($"Disconnection happened, type: {info.Exception.Message}"));
                    
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

                    while (_client.IsRunning)
                    {
                        // Essentially an infinite loop while the client is doing something
                    }
                }
            }

        #endregion
        
    }
}