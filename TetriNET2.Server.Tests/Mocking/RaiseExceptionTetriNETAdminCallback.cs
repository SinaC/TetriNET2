using System;
using System.Collections.Generic;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Tests.Mocking
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
        public void OnClientConnected(Guid clientId, string name, string team, string address)
        {
            throw new NotImplementedException();
        }
        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            throw new NotImplementedException();
        }
        public void OnAdminConnected(Guid adminId, string name, string address)
        {
            throw new NotImplementedException();
        }
        public void OnAdminDisconnected(Guid adminId, LeaveReasons reason)
        {
            throw new NotImplementedException();
        }
        public void OnGameCreated(bool createdByClient, Guid clientOrAdminId, GameAdminData game)
        {
            throw new NotImplementedException();
        }
        public void OnGameDeleted(Guid adminId, Guid gameId)
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
        public void OnClientListReceived(List<ClientAdminData> clients)
        {
            throw new NotImplementedException();
        }
        public void OnClientListInGameReceived(Guid gameId, List<ClientAdminData> clients)
        {
            throw new NotImplementedException();
        }
        public void OnGameListReceived(List<GameAdminData> games)
        {
            throw new NotImplementedException();
        }
        public void OnBannedListReceived(List<BanEntryData> entries)
        {
            throw new NotImplementedException();
        }
    }
}
