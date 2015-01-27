using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;

namespace TetriNET2.Server.WCFHost
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public sealed partial class WCFHost : IHost, IDisposable
    {
        private ServiceHost _serviceHost;

        public int Port { get; set; }

        public WCFHost(IBanManager banManager, IClientManager clientManager, IAdminManager adminManager, IGameManager gameManager)
        {
            BanManager = banManager;
            ClientManager = clientManager;
            AdminManager = adminManager;
            GameManager = gameManager;
        }

        #region IHost

        public IBanManager BanManager { get; private set; }
        public IClientManager ClientManager { get; private set; }
        public IGameManager GameManager { get; private set; }
        public IAdminManager AdminManager { get; private set; }

        public void Start()
        {
            Uri baseAddress = new Uri(String.Format("net.tcp://localhost:{0}", 7788));

            _serviceHost = new ServiceHost(this, baseAddress);
            _serviceHost.AddServiceEndpoint(typeof(ITetriNETClient), new NetTcpBinding(SecurityMode.None), "/TetriNET2Client");
            _serviceHost.AddServiceEndpoint(typeof(ITetriNETAdmin), new NetTcpBinding(SecurityMode.None), "/TetriNET2Admin");
            _serviceHost.Open();

            Log.Default.WriteLine(LogLevels.Info, "WCF Client/Admin Host opened on {0}", baseAddress);

            foreach (var endpt in _serviceHost.Description.Endpoints)
            {
                Log.Default.WriteLine(LogLevels.Debug, "Enpoint address:\t{0}", endpt.Address);
                Log.Default.WriteLine(LogLevels.Debug, "Enpoint binding:\t{0}", endpt.Binding);
                Log.Default.WriteLine(LogLevels.Debug, "Enpoint contract:\t{0}", endpt.Contract.ContractType.Name);
            }
        }

        public void Stop()
        {
            // Close service host
            _serviceHost.Do(x => x.Close());
        }

        public void AddClient(IClient added)
        {
            // NOP
        }

        public void AddAdmin(IAdmin added)
        {
            // NOP
        }

        public void AddGame(IGame added)
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

        public void RemoveGame(IGame removed)
        {
            // NOP
        }

        #endregion

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_serviceHost != null)
                    _serviceHost.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        private IPAddress Address
        {
            get
            {
                MessageProperties messageProperties = OperationContext.Current.IncomingMessageProperties;
                RemoteEndpointMessageProperty endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                if (endpointProperty != null)
                    return IPAddress.Parse(endpointProperty.Address);
                return null;
            }
        }
    }
}
