using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using SmartContract.Channels;

namespace SmartContract.Controllers
{
    public class MinerSocketController : ControllerBase
    {
        private static readonly Channel Channel = new();

        [HttpGet(("/miners/connect/ws"))]
        public async Task Connect()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await Channel.Listen(webSocket);
        }

        [HttpGet("/miners")]
        public IActionResult Index()
        {
            var jsonObject = new JsonObject();
            jsonObject["miners"] = JsonSerializer.SerializeToNode(Channel.GetConnectedSockets()); 
            return Ok(jsonObject.ToJsonString());
        }

        [HttpPost("/miners/broadcast")]
        public IActionResult Broadcast()
        {
            var jsonObject = new JsonObject
            {
                { "message", "Hello world!" }
            };
            
            Channel.Broadcast("miner", "broadcast:restful", jsonObject);

            return Ok();
        }
        
    }
}