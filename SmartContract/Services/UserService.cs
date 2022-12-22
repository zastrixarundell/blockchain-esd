using SmartContract.Services.Interfaces;

namespace SmartContract.Services;

public class UserService : IUserService
{
    private static IEnumerable<User> _requests = new List<User>();

    public bool QueueUser(User user)
    {
        if (_requests.Any(u => u.Id.Equals(user.Id)))
            return false;
            
        _requests = _requests.Append(user);
        return true;
    }

    public IEnumerable<User> GetAll()
    {
        return _requests;
    }
}