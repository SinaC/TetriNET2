﻿using System;
using System.Collections.Generic;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Admin.Interfaces
{
    public delegate void ConnectionLostEventHandler();

    public delegate void ConnectedEventHandler(ConnectResults result, Versioning serverVersion, Guid adminId);
    public delegate void DisconnectedEventHandler();

    public delegate void ServerStoppedEventHandler();

    public delegate void ClientConnectedEventHandler(Guid clientId, string name, string team);
    public delegate void ClientDisconnectedEventHandler(Guid clientId, LeaveReasons reason);

    public delegate void AdminConnectedEventHandler(Guid adminId, string name);
    public delegate void AdminDisconnectedEventHandler(Guid adminId, LeaveReasons reason);

    public delegate void GameCreatedEventHandler(bool createdByClient, Guid clientOrAdminId, GameAdminData game);
    public delegate void GameDeletedEventHandler(Guid adminId, Guid gameId);

    public delegate void ServerMessageReceivedEventHandler(string message);
    public delegate void BroadcastMessageReceivedEventHandler(Guid clientId, string message);
    public delegate void PrivateMessageReceivedEventHandler(Guid adminId, string message);

    public delegate void AdminListReceivedEventHandler(List<AdminData> admins);
    public delegate void ClientListReceivedEventHandler(List<ClientAdminData> clients);
    public delegate void ClientListInGameReceivedEventHandler(Guid gameId, List<ClientAdminData> clients);
    public delegate void GameListReceivedEventHandler(List<GameAdminData> games);
    public delegate void BannedListReceivedEventHandler(List<BanEntryData> entries);

    public interface IAdmin : ITetriNETAdminCallback
    {
        string Name { get; }
        Versioning Version { get; }

        // Following list are updated internally with ITetriNETAdminCallback notifications
        IReadOnlyCollection<ClientAdminData> Clients { get; }
        IReadOnlyCollection<AdminData> Admins { get; }
        IReadOnlyCollection<GameAdminData> Games { get; }
        IReadOnlyCollection<BanEntryData> Banned { get; }

        //
        void SetVersion(int major, int minor);

        //
        event ConnectionLostEventHandler ConnectionLost;

        //
        event ConnectedEventHandler Connected;
        event DisconnectedEventHandler Disconnected;

        event ServerStoppedEventHandler ServerStopped;

        event ClientConnectedEventHandler ClientConnected;
        event ClientDisconnectedEventHandler ClientDisconnected;

        event AdminConnectedEventHandler AdminConnected;
        event AdminDisconnectedEventHandler AdminDisconnected;

        event GameCreatedEventHandler GameCreated;
        event GameDeletedEventHandler GameDeleted;

        event ServerMessageReceivedEventHandler ServerMessageReceived;
        event BroadcastMessageReceivedEventHandler BroadcastMessageReceived;
        event PrivateMessageReceivedEventHandler PrivateMessageReceived;

        event AdminListReceivedEventHandler AdminListReceived;
        event ClientListReceivedEventHandler ClientListReceived;
        event ClientListInGameReceivedEventHandler ClientListInGameReceived;
        event GameListReceivedEventHandler GameListReceived;
        event BannedListReceivedEventHandler BannedListReceived;

        // Connect/disconnect
        bool Connect(string address, string name, string password);
        bool Disconnect();

        // Messaging
        bool SendPrivateAdminMessage(Guid targetAdminId, string message);
        bool SendPrivateMessage(Guid targetClientId, string message);
        bool SendBroadcastMessage(string message);

        // Monitoring
        bool GetAdminList();
        bool GetClientList();
        bool GetClientListInGame(Guid gameId);
        bool GetGameList();
        bool GetBannedList();

        // Kick/Ban
        bool Kick(Guid targetId, string reason);
        bool Ban(Guid targetId, string reason);

        // Server commands
        bool RestartServer(int seconds);
    }
}
