using System.Threading.Tasks;
using Websocket.Client;

namespace Miner.Services
{
    public interface IMinerSocket
    {
        void Register(Miner miner);

        void HandleMessage(ResponseMessage message);
    }
}