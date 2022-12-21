namespace SmartContract.Services.Interfaces;

public interface IUserService
{
    public IEnumerable<User> GetAll();
    public bool QueueUser(User user);
}