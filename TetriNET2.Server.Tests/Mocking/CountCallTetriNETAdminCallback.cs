using System;
using System.Collections.Generic;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Tests.Mocking
{
    public class CountCallTetriNETAdminCallback : ITetriNETAdminCallback
    {
        private readonly Dictionary<string, int> _callCount = new Dictionary<string, int>();

        private void UpdateCallCount(string callbackName)
        {
            if (!_callCount.ContainsKey(callbackName))
                _callCount.Add(callbackName, 1);
            else
                _callCount[callbackName]++;
        }

        public int GetCallCount(string callbackName)
        {
            int value;
            _callCount.TryGetValue(callbackName, out value);
            return value;
        }

        public void Reset()
        {
            _callCount.Clear();
        }

        #region ITetriNETAdminCallback

        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid adminId)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnDisconnected()
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnServerStopped()
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientConnected(Guid clientId, string name, string team, string address)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnAdminConnected(Guid adminId, string name, string address)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnAdminDisconnected(Guid adminId, LeaveReasons reason)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameCreated(Guid clientId, GameRoomAdminData game)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnServerMessageReceived(string message)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPrivateMessageReceived(Guid adminId, string message)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnAdminListReceived(List<AdminData> admins)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientListReceived(List<ClientAdminData> clients)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientListInRoomReceived(Guid roomId, List<ClientAdminData> clients)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnRoomListReceived(List<GameRoomAdminData> rooms)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnBannedListReceived(List<BanEntryData> entries)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        #endregion
    }
}
