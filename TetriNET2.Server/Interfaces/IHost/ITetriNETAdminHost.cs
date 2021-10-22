using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces.IHost
{
    // ------
    // Admin
    // Connect/Disconnect
    public delegate void AdminConnectEventHandler(ITetriNETAdminCallback callback, IAddress address, Versioning version, string name, string password);
    public delegate void AdminDisconnectEventHandler(IAdmin admin);

    // Messaging
    public delegate void AdminSendPrivateAdminMessageEventHandler(IAdmin admin, IAdmin target, string message);
    public delegate void AdminSendPrivateMessageEventHandler(IAdmin admin, IClient target, string message);
    public delegate void AdminSendBroadcastMessageEventHandler(IAdmin admin, string message);

    // Monitoring
    public delegate void AdminGetAdminListEventHandler(IAdmin admin);
    public delegate void AdminGetClientListEventHandler(IAdmin admin);
    public delegate void AdminGetClientListInGameEventHandler(IAdmin admin, IGame game);
    public delegate void AdminGetGameListEventHandler(IAdmin admin);
    public delegate void AdminGetBannedListEventHandler(IAdmin admin);

    public delegate void AdminCreateGameEventHandler(IAdmin admin, string name, GameRules rule, string password);
    public delegate void AdminDeleteGameEventHandler(IAdmin admin, IGame game);

    // Kick/Ban
    public delegate void AdminKickEventHandler(IAdmin admin, IClient target, string reason);
    public delegate void AdminBanEventHandler(IAdmin admin, IClient target, string reason);

    // Server commands
    public delegate void AdminRestartServerEventHandler(IAdmin admin, int seconds);

    public partial interface IHost : ITetriNETAdmin
    {
        // ------
        // Admin
        // Connect/Disconnect
        event AdminConnectEventHandler HostAdminConnect;
        event AdminDisconnectEventHandler HostAdminDisconnect;

        // Messaging
        event AdminSendPrivateAdminMessageEventHandler HostAdminSendPrivateAdminMessage;
        event AdminSendPrivateMessageEventHandler HostAdminSendPrivateMessage;
        event AdminSendBroadcastMessageEventHandler HostAdminSendBroadcastMessage;

        // Monitoring
        event AdminGetAdminListEventHandler HostAdminGetAdminList;
        event AdminGetClientListEventHandler HostAdminGetClientList;
        event AdminGetClientListInGameEventHandler HostAdminGetClientListInGame;
        event AdminGetGameListEventHandler HostAdminGetGameList;
        event AdminGetBannedListEventHandler HostAdminGetBannedList;

        // Game
        event AdminCreateGameEventHandler HostAdminCreateGame;
        event AdminDeleteGameEventHandler HostAdminDeleteGame;

        // Kick/Ban
        event AdminKickEventHandler HostAdminKick;
        event AdminBanEventHandler HostAdminBan;

        // Server commands
        event AdminRestartServerEventHandler HostAdminRestartServer;
    }
}
