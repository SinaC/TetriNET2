using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces.IHost
{
    // ------
    // Admin
    // Connect/Disconnect
    public delegate void HostAdminConnectEventHandler(ITetriNETAdminCallback callback, IPAddress address, Versioning version, string name, string password);
    public delegate void HostAdminDisconnectEventHandler(IAdmin admin);

    // Messaging
    public delegate void HostAdminSendPrivateAdminMessageEventHandler(IAdmin admin, IAdmin target, string message);
    public delegate void HostAdminSendPrivateMessageEventHandler(IAdmin admin, IClient target, string message);
    public delegate void HostAdminSendBroadcastMessageEventHandler(IAdmin admin, string message);

    // Monitoring
    public delegate void HostAdminGetAdminListEventHandler(IAdmin admin);
    public delegate void HostAdminGetClientListEventHandler(IAdmin admin);
    public delegate void HostAdminGetClientListInGameEventHandler(IAdmin admin, IGame game);
    public delegate void HostAdminGetGameListEventHandler(IAdmin admin);
    public delegate void HostAdminGetBannedListEventHandler(IAdmin admin);

    public delegate void HostAdminCreateGameEventHandler(IAdmin admin, string name, GameRules rule, string password);
    public delegate void HostAdminDeleteGameEventHandler(IAdmin admin, IGame game);

    // Kick/Ban
    public delegate void HostAdminKickEventHandler(IAdmin admin, IClient target, string reason);
    public delegate void HostAdminBanEventHandler(IAdmin admin, IClient target, string reason);

    // Server commands
    public delegate void HostAdminRestartServerEventHandler(IAdmin admin, int seconds);

    public partial interface IHost : ITetriNETAdmin
    {
        // ------
        // Admin
        // Connect/Disconnect
        event HostAdminConnectEventHandler HostAdminConnect;
        event HostAdminDisconnectEventHandler HostAdminDisconnect;

        // Messaging
        event HostAdminSendPrivateAdminMessageEventHandler HostAdminSendPrivateAdminMessage;
        event HostAdminSendPrivateMessageEventHandler HostAdminSendPrivateMessage;
        event HostAdminSendBroadcastMessageEventHandler HostAdminSendBroadcastMessage;

        // Monitoring
        event HostAdminGetAdminListEventHandler HostAdminGetAdminList;
        event HostAdminGetClientListEventHandler HostAdminGetClientList;
        event HostAdminGetClientListInGameEventHandler HostAdminGetClientListInGame;
        event HostAdminGetGameListEventHandler HostAdminGetGameList;
        event HostAdminGetBannedListEventHandler HostAdminGetBannedList;

        // Game
        event HostAdminCreateGameEventHandler HostAdminCreateGame;
        event HostAdminDeleteGameEventHandler HostAdminDeleteGame;

        // Kick/Ban
        event HostAdminKickEventHandler HostAdminKick;
        event HostAdminBanEventHandler HostAdminBan;

        // Server commands
        event HostAdminRestartServerEventHandler HostAdminRestartServer;
    }
}
