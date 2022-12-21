using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartContract.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : Controller
    {

        // POST: UserController/Create
        [HttpPost]
        public string Create(User user)
        {
            try
            {
                return "Ok";
            }
            catch
            {
                return "Error";
            }
        }
    }
}
