using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;

namespace TetriNET2.Tests.Server.ClientSide
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

        #region ITetriNETAdmin

        public void AdminConnect(string password)
        {
            Host.AdminConnect(this, Versioning, Name, password);
        }

        public void AdminDisconnect()
        {
            Host.AdminDisconnect(this);
        }

        public void AdminSendPrivateAdminMessage(Guid targetAdminId, string message)
        {
            Host.AdminSendPrivateAdminMessage(this, targetAdminId, message);
        }

        public void AdminSendPrivateMessage(Guid targetClientId, string message)
        {
            Host.AdminSendPrivateMessage(this, targetClientId, message);
        }

        public void AdminSendBroadcastMessage(string message)
        {
            Host.AdminSendBroadcastMessage(this, message);
        }

        public void AdminGetAdminList()
        {
            Host.AdminGetAdminList(this);
        }

        public void AdminGetClientList()
        {
            Host.AdminGetClientList(this);
        }

        public void AdminGetClientListInRoom(Guid roomId)
        {
            Host.AdminGetClientListInRoom(this, roomId);
        }

        public void AdminGetRoomList()
        {
            Host.AdminGetRoomList(this);
        }

        public void AdminGetBannedList()
        {
            Host.AdminGetBannedList(this);
        }

        public void AdminKick(Guid targetId, string reason)
        {
            Host.AdminKick(this, targetId, reason);
        }

        public void AdminBan(Guid targetId, string reason)
        {
            Host.AdminBan(this, targetId, reason);
        }

        public void AdminRestartServer(int seconds)
        {
            Host.AdminRestartServer(this, seconds);
        }

        #endregion

        #region ITetriNETAdminCallback

        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid adminId)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnDisconnected()
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnServerStopped()
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientConnected(Guid clientId, string name, string team)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnAdminConnected(Guid adminId, string name)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnAdminDisconnected(Guid adminId, LeaveReasons reason)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameCreated(Guid clientId, GameDescription game)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnServerMessageReceived(string message)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPrivateMessageReceived(Guid adminId, string message)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnAdminListReceived(List<AdminData> admins)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientListReceived(List<ClientData> clients)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientListInRoomReceived(Guid roomId, List<ClientData> clients)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnRoomListReceived(List<GameRoomData> rooms)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnBannedListReceived(List<BanEntryData> entries)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
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
