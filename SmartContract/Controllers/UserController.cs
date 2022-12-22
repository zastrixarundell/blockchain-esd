using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using SmartContract.Services;
using SmartContract.Services.Interfaces;

namespace SmartContract.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : Controller
    {
        // POST: UserController/Create
        [HttpPost(Name = "Create user connection")]
        public IActionResult Create(User user)
        {
            var jsonObject = new JsonObject();
            int code;

            if (Manager.UserService.QueueUser(user))
            {
                jsonObject["status"] = "ok";
                jsonObject["message"] = "User request added to queue.";
                code = 201;
            }
            else
            {
                jsonObject["status"] = "error";
                jsonObject["message"] = $"User with ID \"{user.Id}\" has already been taken!";
                return BadRequest(jsonObject.ToJsonString());
                code = 400;
            }

            return StatusCode(code, jsonObject.ToJsonString());
        }

        [HttpGet(Name = "List users/requests")]
        public IActionResult Index()
        {
            var jsonObject = new JsonObject();;
            jsonObject["users"] = JsonSerializer.SerializeToNode(Manager.UserService.GetAll()); 
            return Ok(jsonObject.ToJsonString());
        }
    }
}
