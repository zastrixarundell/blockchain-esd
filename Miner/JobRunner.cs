using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Miner
{
    public class JobRunner
    {
        private readonly string _request;

        public JobRunner(string request)
        {
            _request = request;
        }

        public Task<string> CalculateHash()
        {
            return Task.Run(() =>
            {
                var sha256Hash = SHA256.Create();
                var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(_request));

                var builder = new StringBuilder();
                foreach (var byteData in bytes)
                    builder.Append(byteData.ToString("x2"));

                return builder.ToString();
            });
        }
    }
}