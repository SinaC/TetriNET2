using System;
using System.Collections.Generic;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Admin.Interfaces
{
    public delegate void AdminConnectionLostEventHandler();

    public delegate void AdminOnConnectedEventHandler(ConnectResults result, Versioning serverVersion, Guid adminId);
    public delegate void AdminOnDisconnectedEventHandler();

    public delegate void AdminOnServerStoppedEventHandler();

    public delegate void AdminOnClientConnectedEventHandler(Guid clientId, string name, string team);
    public delegate void AdminOnClientDisconnectedEventHandler(Guid clientId, LeaveReasons reason);

    public delegate void AdminOnAdminConnectedEventHandler(Guid adminId, string name);
    public delegate void AdminOnAdminDisconnectedEventHandler(Guid adminId, LeaveReasons reason);

    public delegate void AdminOnGameCreatedEventHandler(Guid clientId, GameDescription game);

    public delegate void AdminOnServerMessageReceivedEventHandler(string message);
    public delegate void AdminOnBroadcastMessageReceivedEventHandler(Guid clientId, string message);
    public delegate void AdminOnPrivateMessageReceivedEventHandler(Guid adminId, string message);

    public delegate void AdminOnAdminListReceivedEventHandler(List<AdminData> admins);
    public delegate void AdminOnClientListReceivedEventHandler(List<ClientAdminData> clients);
    public delegate void AdminOnClientListInRoomReceivedEventHandler(Guid roomId, List<ClientAdminData> clients);
    public delegate void AdminOnRoomListReceivedEventHandler(List<GameRoomAdminData> rooms);
    public delegate void AdminOnBannedListReceivedEventHandler(List<BanEntryData> entries);

    public interface IAdmin : ITetriNETAdminCallback
    {
        string Name { get; }
        Versioning Version { get; }

        // Following list are updated internally with ITetriNETAdminCallback notifications
        IEnumerable<ClientAdminData> Clients { get; }
        IEnumerable<AdminData> Admins { get; }
        IEnumerable<GameRoomAdminData> Rooms { get; }

        //
        void SetVersion(int major, int minor);

        //
        event AdminConnectionLostEventHandler AdminConnectionLost;

        //
        event AdminOnConnectedEventHandler AdminOnConnected;
        event AdminOnDisconnectedEventHandler AdminOnDisconnected;

        event AdminOnServerStoppedEventHandler AdminOnServerStopped;

        event AdminOnClientConnectedEventHandler AdminOnClientConnected;
        event AdminOnClientDisconnectedEventHandler AdminOnClientDisconnected;

        event AdminOnAdminConnectedEventHandler AdminOnAdminConnected;
        event AdminOnAdminDisconnectedEventHandler AdminOnAdminDisconnected;

        event AdminOnGameCreatedEventHandler AdminOnGameCreated;

        event AdminOnServerMessageReceivedEventHandler AdminOnServerMessageReceived;
        event AdminOnBroadcastMessageReceivedEventHandler AdminOnBroadcastMessageReceived;
        event AdminOnPrivateMessageReceivedEventHandler AdminOnPrivateMessageReceived;

        event AdminOnAdminListReceivedEventHandler AdminOnAdminListReceived;
        event AdminOnClientListReceivedEventHandler AdminOnClientListReceived;
        event AdminOnClientListInRoomReceivedEventHandler AdminOnClientListInRoomReceived;
        event AdminOnRoomListReceivedEventHandler AdminOnRoomListReceived;
        event AdminOnBannedListReceivedEventHandler AdminOnBannedListReceived;

        // Connect/disconnect
        bool Connect(string address, Versioning version, string name, string password);
        bool Disconnect();

        // Messaging
        bool SendPrivateAdminMessage(Guid targetAdminId, string message);
        bool SendPrivateMessage(Guid targetClientId, string message);
        bool SendBroadcastMessage(string message);

        // Monitoring
        bool GetAdminList();
        bool GetClientList();
        bool GetClientListInRoom(Guid roomId);
        bool GetRoomList();
        bool GetBannedList();

        // Kick/Ban
        bool Kick(Guid targetId, string reason);
        bool Ban(Guid targetId, string reason);

        // Server commands
        bool RestartServer(int seconds);
    }
}
