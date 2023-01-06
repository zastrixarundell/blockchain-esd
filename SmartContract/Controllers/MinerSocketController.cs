using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartContract.Channels;
using System.Linq;

namespace SmartContract.Controllers
{
    public class MinerSocketController : ControllerBase
    {
        [HttpGet(("/miners/connect/ws"))]
        public async Task Connect()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await Manager.MinerChannel.Listen(webSocket);
        }

        [HttpGet("/miners")]
        public IActionResult Index()
        {
            var jsonObject = new JsonObject
            {
                { "miners", Manager.MinerChannel.GetConnectedMiners().ToList().Count }
            };
              
            return Ok(jsonObject.ToJsonString());
        }

        [HttpPost("/miners/broadcast")]
        public IActionResult Broadcast()
        {
            var jsonObject = new JsonObject
            {
                { "message", "Hello world!" }
            };

            Manager.MinerChannel.Broadcast("miner", "broadcast:restful", jsonObject);

            return Ok();
        }
    }
}
