using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Server.Interfaces.IHost;
using TetriNET2.Server.Tests.Mocking;

namespace TetriNET2.Server.Tests.ClientSide
{
    public class AdminFake : ITetriNETAdminCallback
    {
        public IHost Host { get; set; }

        public readonly string Name;
        public readonly Versioning Versioning;
        public readonly IPAddress Address;

        public AdminFake(string name, Versioning version, IPAddress address)
        {
            Name = name;
            Versioning = version;
            Address = address;
        }

        private void SetCallbackAndAddress()
        {
            HostMock hostMock = Host as HostMock;
            if (hostMock != null)
            {
                hostMock.AdminCallback = this;
                hostMock.Address = Address;
            }
        }

        #region ITetriNETAdmin

        public void AdminConnect(string password)
        {
            SetCallbackAndAddress();
            Host.AdminConnect(Versioning, Name, password);
        }

        public void AdminDisconnect()
        {
            SetCallbackAndAddress();
            Host.AdminDisconnect();
        }

        public void AdminSendPrivateAdminMessage(Guid targetAdminId, string message)
        {
            SetCallbackAndAddress();
            Host.AdminSendPrivateAdminMessage(targetAdminId, message);
        }

        public void AdminSendPrivateMessage(Guid targetClientId, string message)
        {
            SetCallbackAndAddress();
            Host.AdminSendPrivateMessage(targetClientId, message);
        }

        public void AdminSendBroadcastMessage(string message)
        {
            SetCallbackAndAddress();
            Host.AdminSendBroadcastMessage(message);
        }

        public void AdminGetAdminList()
        {
            SetCallbackAndAddress();
            Host.AdminGetAdminList();
        }

        public void AdminGetClientList()
        {
            SetCallbackAndAddress();
            Host.AdminGetClientList();
        }

        public void AdminGetClientListInGame(Guid gameId)
        {
            SetCallbackAndAddress();
            Host.AdminGetClientListInGame(gameId);
        }

        public void AdminGetGameList()
        {
            SetCallbackAndAddress();
            Host.AdminGetGameList();
        }

        public void AdminGetBannedList()
        {
            SetCallbackAndAddress();
            Host.AdminGetBannedList();
        }

        public void AdminCreateGame(string name, GameRules rule, string password)
        {
            SetCallbackAndAddress();
            Host.AdminCreateGame(name, rule, password);
        }

        public void AdminDeleteGame(Guid gameId)
        {
            SetCallbackAndAddress();
            Host.AdminDeleteGame(gameId);
        }
        
        public void AdminKick(Guid targetId, string reason)
        {
            SetCallbackAndAddress();
            Host.AdminKick(targetId, reason);
        }

        public void AdminBan(Guid targetId, string reason)
        {
            SetCallbackAndAddress();
            Host.AdminBan(targetId, reason);
        }

        public void AdminRestartServer(int seconds)
        {
            SetCallbackAndAddress();
            Host.AdminRestartServer(seconds);
        }

        #endregion

        #region ITetriNETAdminCallback

        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid adminId)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, result, serverVersion, adminId);
        }

        public void OnDisconnected()
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnServerStopped()
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientConnected(Guid clientId, string name, string team, string address)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, name, team, address);
        }

        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, reason);
        }

        public void OnAdminConnected(Guid adminId, string name, string address)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, address, name, address);
        }

        public void OnAdminDisconnected(Guid adminId, LeaveReasons reason)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, adminId, reason);
        }

        public void OnGameCreated(bool createdByClient, Guid clientOrAdminId, GameAdminData game)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, createdByClient, clientOrAdminId, game);
        }

        public void OnGameDeleted(Guid adminId, Guid gameId)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, adminId, gameId);
        }

        public void OnServerMessageReceived(string message)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, message);
        }

        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, message);
        }

        public void OnPrivateMessageReceived(Guid adminId, string message)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, adminId, message);
        }

        public void OnAdminListReceived(List<AdminData> admins)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, admins);
        }

        public void OnClientListReceived(List<ClientAdminData> clients)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clients);
        }

        public void OnClientListInGameReceived(Guid gameId, List<ClientAdminData> clients)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, gameId, clients);
        }

        public void OnGameListReceived(List<GameAdminData> games)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, games);
        }

        public void OnBannedListReceived(List<BanEntryData> entries)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, entries);
        }

        #endregion

        #region Call Info

        private class CallInfo
        {
            public static readonly CallInfo NullObject = new CallInfo
            {
                Count = 0,
                ParametersPerCall = null
            };

            public int Count { get; set; }
            public List<List<object>> ParametersPerCall { get; set; }
        }

        private readonly Dictionary<string, CallInfo> _callInfos = new Dictionary<string, CallInfo>();

        private void UpdateCallInfo(string callbackName, params object[] parameters)
        {
            List<object> paramList = parameters == null ? new List<object>() : parameters.ToList();
            if (!_callInfos.ContainsKey(callbackName))
                _callInfos.Add(callbackName, new CallInfo
                {
                    Count = 1,
                    ParametersPerCall = new List<List<object>> { paramList }
                });
            else
            {
                CallInfo callInfo = _callInfos[callbackName];
                callInfo.Count++;
                callInfo.ParametersPerCall.Add(paramList);
            }
        }

        public int GetCallCount(string callbackName)
        {
            CallInfo value;
            _callInfos.TryGetValue(callbackName, out value);
            return (value ?? CallInfo.NullObject).Count;
        }

        public List<object> GetCallParameters(string callbackName, int callId)
        {
            CallInfo value;
            if (!_callInfos.TryGetValue(callbackName, out value))
                return null;
            if (callId >= value.Count)
                return null;
            return value.ParametersPerCall[callId];
        }

        public void ResetCallInfo()
        {
            _callInfos.Clear();
        }

        #endregion
    }
}
