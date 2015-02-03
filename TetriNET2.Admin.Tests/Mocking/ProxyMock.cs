using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TetriNET2.Admin.Interfaces;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Admin.Tests.Mocking
{
    public class ProxyMock : IProxy, ICallCount
    {
        #region ICallCount

        private readonly Dictionary<string, int> _callCount = new Dictionary<string, int>();

        private void UpdateCallCount([CallerMemberName]string callbackName = null)
        {
            if (String.IsNullOrWhiteSpace(callbackName))
                return;
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

        #endregion

        public ProxyMock(ITetriNETAdminCallback callback, string address, Versioning version)
        {
        }

        public void AdminConnect(Versioning version, string name, string password)
        {
            UpdateCallCount();
        }
        public void AdminDisconnect()
        {
            UpdateCallCount();
        }
        public void AdminSendPrivateAdminMessage(Guid targetAdminId, string message)
        {
            UpdateCallCount();
        }
        public void AdminSendPrivateMessage(Guid targetClientId, string message)
        {
            UpdateCallCount();
        }
        public void AdminSendBroadcastMessage(string message)
        {
            UpdateCallCount();
        }
        public void AdminGetAdminList()
        {
            UpdateCallCount();
        }
        public void AdminGetClientList()
        {
            UpdateCallCount();
        }
        public void AdminGetClientListInGame(Guid gameId)
        {
            UpdateCallCount();
        }
        public void AdminGetGameList()
        {
            UpdateCallCount();
        }
        public void AdminGetBannedList()
        {
            UpdateCallCount();
        }
        public void AdminCreateGame(string name, GameRules rule, string password)
        {
            UpdateCallCount();
        }
        public void AdminDeleteGame(Guid gameId)
        {
            UpdateCallCount();
        }
        public void AdminKick(Guid targetId, string reason)
        {
            UpdateCallCount();
        }
        public void AdminBan(Guid targetId, string reason)
        {
            UpdateCallCount();
        }
        public void AdminRestartServer(int seconds)
        {
            UpdateCallCount();
        }
        
        public event ProxyAdminConnectionLostEventHandler ConnectionLost;
        
        public bool Disconnect()
        {
            UpdateCallCount();
            return true;
        }
    }
}
