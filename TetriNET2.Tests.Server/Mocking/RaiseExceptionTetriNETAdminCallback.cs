using System;
using System.Collections.Generic;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Tests.Server.Mocking
{
    public class RaiseExceptionTetriNETAdminCallback : ITetriNETAdminCallback
    {
        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid adminId)
        {
            throw new NotImplementedException();
        }
        public void OnDisconnected()
        {
            throw new NotImplementedException();
        }
        public void OnServerStopped()
        {
            throw new NotImplementedException();
        }
        public void OnClientConnected(Guid clientId, string name, string team)
        {
            throw new NotImplementedException();
        }
        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            throw new NotImplementedException();
        }
        public void OnAdminConnected(Guid adminId, string name)
        {
            throw new NotImplementedException();
        }
        public void OnAdminDisconnected(Guid adminId)
        {
            throw new NotImplementedException();
        }
        public void OnGameCreated(Guid clientId, GameDescription game)
        {
            throw new NotImplementedException();
        }
        public void OnServerMessageReceived(string message)
        {
            throw new NotImplementedException();
        }
        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            throw new NotImplementedException();
        }
        public void OnPrivateMessageReceived(Guid adminId, string message)
        {
            throw new NotImplementedException();
        }
        public void OnAdminListReceived(List<AdminData> admins)
        {
            throw new NotImplementedException();
        }
        public void OnClientListReceived(List<ClientData> clients)
        {
            throw new NotImplementedException();
        }
        public void OnClientListInRoomReceived(Guid roomId, List<ClientData> clients)
        {
            throw new NotImplementedException();
        }
        public void OnRoomListReceived(List<GameRoomData> rooms)
        {
            throw new NotImplementedException();
        }
        public void OnBannedListReceived(List<BanEntryData> entries)
        {
            throw new NotImplementedException();
        }
    }
}
