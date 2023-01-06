using System.Threading.Tasks;
using Websocket.Client;

namespace Miner.Services
{
    public abstract class MinerSocket
    {
        protected readonly Miner Miner;

        protected MinerSocket(Miner miner)
        {
            Miner = miner;
        }
        
        public abstract void Register();
    }
}