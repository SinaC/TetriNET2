﻿using System;
using System.Collections.Generic;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Common.Contracts
{
    public interface ITetriNETAdminCallback
    {
        void OnConnected(ConnectResults result, Versioning serverVersion, Guid adminId);
        void OnDisconnected();

        void OnServerStopped();

        void OnClientConnected(Guid clientId, string name, string team);
        void OnClientDisconnected(Guid clientId, LeaveReasons reason);

        void OnAdminConnected(Guid adminId, string name);
        void OnAdminDisconnected(Guid adminId);

        void OnGameCreated(Guid clientId, GameDescription game);

        void OnServerMessageReceived(string message);
        void OnBroadcastMessageReceived(Guid clientId, string message);
        void OnPrivateMessageReceived(Guid adminId, string message);

        void OnAdminListReceived(List<Admin> admins);
        void OnClientListReceived(List<Client> clients);
        void OnClientListInRoomReceived(Guid roomId, List<Client> clients);
        void OnRoomListReceived(List<GameRoom> rooms);
        void OnBannedListReceived(List<BanEntry> entries);
    }
}