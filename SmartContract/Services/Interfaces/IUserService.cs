using System.Collections.Generic;

namespace SmartContract.Services.Interfaces
{
    public interface IUserService
    {
        public IEnumerable<User> GetAll();
        public bool QueueUser(User user);

        public void ClearUsers();

        public void RemoveFromQueue(User user);
    }
}