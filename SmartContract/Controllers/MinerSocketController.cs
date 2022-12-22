using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using SmartContract.Channels;

namespace SmartContract.Controllers
{
    public class MinerSocketController : ControllerBase
    {
        private readonly MinerChannel _channel;
        public MinerSocketController()
        {
            _channel = new MinerChannel();
        }
        
        [HttpGet(("/miner/connect/ws"))]
        public async Task Connect()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _channel.Listen(webSocket);
        }
    }
}