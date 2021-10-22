using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server
{
    public sealed class Admin : IAdmin
    {
        private bool _disconnected;

        public Admin(string name, IAddress address, ITetriNETAdminCallback callback)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            ConnectTime = DateTime.Now;
            _disconnected = false;
        }

        private void ExceptionFreeAction(Action action, [CallerMemberName]string actionName = null)
        {
            try
            {
                if (!_disconnected)
                    action();
            }
            catch (CommunicationObjectAbortedException)
            {
                _disconnected = true;
                ConnectionLost.Do(x => x(this));
            }
            catch (Exception ex)
            {
                _disconnected = true;
                Log.Default.WriteLine(LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                ConnectionLost.Do(x => x(this));
            }
        }

        #region IAdmin

        public event AdminConnectionLostEventHandler ConnectionLost;

        public Guid Id { get; }
        public string Name { get; }
        public IAddress Address { get; }
        public ITetriNETAdminCallback Callback { get; }
        public DateTime ConnectTime { get; }

        #endregion

        #region ITetriNETAdminCallback

        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid adminId)
        {
            ExceptionFreeAction(() => Callback.OnConnected(result, serverVersion, adminId));
        }

        public void OnDisconnected()
        {
            ExceptionFreeAction(() => Callback.OnDisconnected());
        }

        public void OnServerStopped()
        {
            ExceptionFreeAction(() => Callback.OnServerStopped());
        }

        public void OnClientConnected(Guid clientId, string name, string team, string address)
        {
            ExceptionFreeAction(() => Callback.OnClientConnected(clientId, name, team, address));
        }

        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            ExceptionFreeAction(() => Callback.OnClientDisconnected(clientId, reason));
        }

        public void OnAdminConnected(Guid adminId, string name, string address)
        {
            ExceptionFreeAction(() => Callback.OnAdminConnected(adminId, name, address));
        }

        public void OnAdminDisconnected(Guid adminId, LeaveReasons reason)
        {
            ExceptionFreeAction(() => Callback.OnAdminDisconnected(adminId, reason));
        }

        public void OnGameCreated(bool createdByClient, Guid clientOrAdminId, GameAdminData game)
        {
            ExceptionFreeAction(() => Callback.OnGameCreated(createdByClient, clientOrAdminId, game));
        }

        public void OnGameDeleted(Guid adminId, Guid gameId)
        {
            ExceptionFreeAction(() => Callback.OnGameDeleted(adminId, gameId));
        }

        public void OnServerMessageReceived(string message)
        {
            ExceptionFreeAction(() => Callback.OnServerMessageReceived(message));
        }

        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            ExceptionFreeAction(() => Callback.OnBroadcastMessageReceived(clientId, message));
        }

        public void OnPrivateMessageReceived(Guid adminId, string message)
        {
            ExceptionFreeAction(() => OnPrivateMessageReceived(adminId, message));
        }

        public void OnAdminListReceived(List<AdminData> admins)
        {
            ExceptionFreeAction(() => Callback.OnAdminListReceived(admins));
        }

        public void OnClientListReceived(List<ClientAdminData> clients)
        {
            ExceptionFreeAction(() => Callback.OnClientListReceived(clients));
        }

        public void OnClientListInGameReceived(Guid gameId, List<ClientAdminData> clients)
        {
            ExceptionFreeAction(() => Callback.OnClientListInGameReceived(gameId, clients));
        }

        public void OnGameListReceived(List<GameAdminData> games)
        {
            ExceptionFreeAction(() => Callback.OnGameListReceived(games));
        }

        public void OnBannedListReceived(List<BanEntryData> entries)
        {
            ExceptionFreeAction(() => Callback.OnBannedListReceived(entries));
        }

        #endregion
    }
}
