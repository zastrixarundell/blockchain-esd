using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using SmartContract.Services;

namespace SmartContract.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : Controller
    {
        private readonly UserService _service;
        public UserController()
        {
            _service = new UserService();
        }

        // POST: UserController/Create
        [HttpPost(Name = "Create user connection")]
        public IActionResult Create(User user)
        {
            var jsonObject = new JsonObject();
            int code;

            if (_service.QueueUser(user))
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
            jsonObject["users"] = JsonSerializer.SerializeToNode(_service.GetAll()); 
            return Ok(jsonObject.ToJsonString());
        }
    }
}
