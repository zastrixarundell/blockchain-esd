using System;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Miner.Services.Implementations
{
    public class SmartContractSocket : IMinerSocket
    {
        private WebsocketClient client;
        private Miner miner;
        
        public void HandleMessage(ResponseMessage message)
        {
            JsonObject? jsonObject = (JsonObject)JsonObject.Parse(message.ToString());

            switch (jsonObject["event"].ToString())
            {
                case "success:join":
                    miner.Uuid = Guid.Parse(jsonObject["data"]["uuid"].ToString());
                    Console.WriteLine($"Registered miner as: {miner.Uuid}");
                    break;
                case "error:join":
                    Console.WriteLine(jsonObject["data"]["message"].ToString());
                    client.Stop(WebSocketCloseStatus.InvalidPayloadData, "It happens");
                    break;
                case "broadcast:restful":
                    Console.WriteLine(jsonObject["data"]["message"].ToString());
                    break;
                default:
                    break;
            }
        }

        public void Register(Miner miner)
        {
            var url = new Uri("ws://localhost:5067/miners/connect/ws");

            this.miner = miner;

            using (client = new WebsocketClient(url))
            {
                // Setting up the client
                
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info =>
                    Console.WriteLine($"Reconnection happened, type: {info.Type}"));
                client.DisconnectionHappened.Subscribe(info =>
                    Console.WriteLine($"Disconnection happened, type: {info.Exception.Message}"));
                
                client.MessageReceived.Subscribe(HandleMessage);
                
                client.Start().Wait();
                
                // Joined on the event!
                
                JsonObject jsonObject = new JsonObject
                {
                    {"topic", "miner"},
                    {"event", "join"}
                };
                
                client.SendInstant(jsonObject.ToJsonString()).Wait();
                
                Console.WriteLine("Hey, I started!");
                
                // Client is now registered

                while (client.IsRunning)
                {
                    // Essentially an infinite loop while the client is doing something
                }
            }
        }


    }
}