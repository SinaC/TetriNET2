using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;

namespace TetriNET2.Server.WCFHost
{
    public partial class WCFHost : IHost
    {
        public partial class WCFServiceHost
        {
        }

        #region IHost

        public IClientManager ClientManager { get; private set; }
        public IGameRoomManager GameRoomManager { get; private set; }
        public IAdminManager AdminManager { get; private set; }

        public void Start()
        {
            // NOP
        }

        public void Stop()
        {
            // NOP
        }

        public void AddClient(IClient added)
        {
            // NOP
        }

        public void AddAdmin(IAdmin added)
        {
            // NOP
        }

        public void AddGameRoom(IGameRoom added)
        {
            // NOP
        }

        public void RemoveClient(IClient removed)
        {
            // NOP
        }

        public void RemoveAdmin(IAdmin removed)
        {
            // NOP
        }

        public void RemoveGameRoom(IGameRoom removed)
        {
            // NOP
        }

        #endregion
    }
}
