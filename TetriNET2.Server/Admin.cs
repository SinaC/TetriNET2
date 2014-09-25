using System;
using System.Collections.Generic;
using System.Net;
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

        public Admin(string name, IPAddress address, ITetriNETAdminCallback callback)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (address == null)
                throw new ArgumentNullException("address");
            if (callback == null)
                throw new ArgumentNullException("callback");

            Id = Guid.NewGuid();
            Name = name;
            Address = address;
            Callback = callback;
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

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public IPAddress Address { get; private set; }
        public ITetriNETAdminCallback Callback { get; private set; }
        public DateTime ConnectTime { get; private set; }

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

        public void OnGameCreated(Guid clientId, GameRoomAdminData game)
        {
            ExceptionFreeAction(() => Callback.OnGameCreated(clientId, game));
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

        public void OnClientListInRoomReceived(Guid roomId, List<ClientAdminData> clients)
        {
            ExceptionFreeAction(() => Callback.OnClientListInRoomReceived(roomId, clients));
        }

        public void OnRoomListReceived(List<GameRoomAdminData> rooms)
        {
            ExceptionFreeAction(() => Callback.OnRoomListReceived(rooms));
        }

        public void OnBannedListReceived(List<BanEntryData> entries)
        {
            ExceptionFreeAction(() => Callback.OnBannedListReceived(entries));
        }

        #endregion
    }
}
