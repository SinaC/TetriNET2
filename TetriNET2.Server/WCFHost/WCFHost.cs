using System;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;

namespace TetriNET2.Server.WCFHost
{
    public partial class WCFHost : IHost, IDisposable
    {
        private readonly WCFClientServiceHost _clientHost;
        private readonly WCFAdminServiceHost _adminHost;

        public int Port { get; set; }

        public WCFHost(IBanManager banManager, IClientManager clientManager, IAdminManager adminManager, IGameRoomManager gameRoomManager)
        {
            BanManager = banManager;
            ClientManager = clientManager;
            AdminManager = adminManager;
            GameRoomManager = gameRoomManager;

            _clientHost = new WCFClientServiceHost(this);
            _adminHost = new WCFAdminServiceHost(this);
        }

        #region IHost

        public IBanManager BanManager { get; private set; }
        public IClientManager ClientManager { get; private set; }
        public IGameRoomManager GameRoomManager { get; private set; }
        public IAdminManager AdminManager { get; private set; }

        public void Start()
        {
            _clientHost.Port = Port;
            _adminHost.Port = Port;

            _clientHost.Start();
            _adminHost.Start();
        }

        public void Stop()
        {
            _clientHost.Stop();
            _adminHost.Stop();
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

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_clientHost != null)
                    _clientHost.Dispose();
                if (_adminHost != null)
                    _adminHost.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
