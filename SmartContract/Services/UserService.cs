using System.Text.Json.Nodes;
using SmartContract.Services.Interfaces;

namespace SmartContract.Services;

public class UserService : IUserService
{
    private IEnumerable<User> _requests = new List<User>();

    public bool QueueUser(User user)
    {
        if (_requests.Any(u => u.Id.Equals(user.Id)))
            return false;
            
        _requests = _requests.Append(user);

        if (_requests.Count() == 1)
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

    public IEnumerable<User> GetAll()
    {
        return _requests;
    }

    public void ClearUsers()
    {
        _requests = new List<User>();
    }
}