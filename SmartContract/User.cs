namespace SmartContract
{
    public class User
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Data { get; set; }
        public static List<User> Requests
        {
            get { return _requests; }
        }
        private static List<User> _requests = new List<User>();

        public static bool QueueRequest(User user)
        {
            if (_requests.Exists(u => u.Id == user.Id))
                return false;
            
            _requests.Add(user);
            return true;
        }
    }
}
