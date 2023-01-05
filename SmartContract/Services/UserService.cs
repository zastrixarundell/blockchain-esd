using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using SmartContract.Services.Interfaces;

namespace SmartContract.Services
{
    public class UserService : IUserService
    {
        private List<User> _requests = new();

        public bool QueueUser(User user)
        {
            if (_requests.Any(u => u.Id.Equals(user.Id)))
                return false;

            _requests.Add(user);

            if (_requests.Count == 1)
            {
                Manager.MinerChannel.Broadcast(
                    "miner",
                    "new_job",
                    new JsonObject
                    {
                    { "request", user.Data },
                    { "sender", user.Id }
                    });
            }

            return true;
        }

        public void RemoveFromQueue(User user)
        {
            _requests.RemoveAll(u => u.Id == user.Id);
        }

        public IEnumerable<User> GetAll()
        {
            return _requests;
        }

        public void ClearUsers()
        {
            _requests = new List<User>();
        }
    }
}