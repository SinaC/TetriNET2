using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces
{
    // ------
    // Client
    // Connect/disconnect/keep alive
    public delegate void ClientConnectEventHandler(ITetriNETCallback callback, IPAddress address, Versioning version, string name, string team);
    public delegate void ClientDisconnectEventHandler(IClient client);
    public delegate void ClientHeartbeatEventHandler(IClient client);

    // Wait+Game room
    public delegate void ClientSendPrivateMessageEventHandler(IClient client, IClient target, string message);
    public delegate void ClientSendBroadcastMessageEventHandler(IClient client, string message);
    public delegate void ClientChangeTeamEventHandler(IClient client, string team);

    // Wait room
    public delegate void ClientJoinGameEventHandler(IClient client, IGameRoom game, string password, bool asSpectator);
    public delegate void ClientJoinRandomGameEventHandler(IClient client, bool asSpectator);
    public delegate void ClientCreateAndJoinGameEventHandler(IClient client, string name, string password, GameRules rule, bool asSpectator);

    // Game room as game master (player or spectator)
    public delegate void ClientStartGameEventHandler(IClient client);
    public delegate void ClientStopGameEventHandler(IClient client);
    public delegate void ClientPauseGameEventHandler(IClient client);
    public delegate void ClientResumeGameEventHandler(IClient client);
    public delegate void ClientChangeOptionsEventHandler(IClient client, GameOptions options);
    public delegate void ClientVoteKickEventHandler(IClient client, IClient target);
    public delegate void ClientVoteKickResponseEventHandler(IClient client, bool accepted);
    public delegate void ClientResetWinListEventHandler(IClient client);

    // Game room as player or spectator
    public delegate void ClientLeaveGameEventHandler(IClient client);

    // Game room as player
    public delegate void ClientPlacePieceEventHandler(IClient client, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid);
    public delegate void ClientModifyGridEventHandler(IClient client, byte[] grid);
    public delegate void ClientUseSpecialEventHandler(IClient client, IClient target, Specials special);
    public delegate void ClientClearLinesEventHandler(IClient client, int count);
    public delegate void ClientGameLostEventHandler(IClient client);
    public delegate void ClientFinishContinuousSpecialEventHandler(IClient client, Specials special);
    public delegate void ClientEarnAchievementEventHandler(IClient client, int achievementId, string achievementTitle);

    // ------
    // Admin
    // Connect/Disconnect
    public delegate void AdminConnectEventHandler(ITetriNETAdminCallback callback, IPAddress address, Versioning version, string name, string password);
    public delegate void AdminDisconnectEventHandler(IAdmin admin);

    // Messaging
    public delegate void AdminSendPrivateAdminMessageEventHandler(IAdmin admin, IAdmin target, string message);
    public delegate void AdminSendPrivateMessageEventHandler(IAdmin admin, IClient target, string message);
    public delegate void AdminSendBroadcastMessageEventHandler(IAdmin admin, string message);

    // Monitoring
    public delegate void AdminGetAdminListEventHandler(IAdmin admin);
    public delegate void AdminGetClientListEventHandler(IAdmin admin);
    public delegate void AdminGetClientListInRoomEventHandler(IAdmin admin, IGameRoom room);
    public delegate void AdminGetRoomListEventHandler(IAdmin admin);
    public delegate void AdminGetBannedListEventHandler(IAdmin admin);

    // Kick/Ban
    public delegate void AdminKickEventHandler(IAdmin admin, IClient target, string reason);
    public delegate void AdminBanEventHandler(IAdmin admin, IClient target, string reason);
    
    // Server commands
    public delegate void AdminRestartServerEventHandler(IAdmin admin, int seconds);

    public interface IHost : ITetriNET, ITetriNETAdmin
    {
        // ------
        // Client
        // Connect/disconnect/keep alive
        event ClientConnectEventHandler ClientConnect;
        event ClientDisconnectEventHandler ClientDisconnect;
        event ClientHeartbeatEventHandler ClientHeartbeat;

        // Wait+Game room
        event ClientSendPrivateMessageEventHandler ClientSendPrivateMessage;
        event ClientSendBroadcastMessageEventHandler ClientSendBroadcastMessage;
        event ClientChangeTeamEventHandler ClientChangeTeam;

        // Wait room
        event ClientJoinGameEventHandler ClientJoinGame;
        event ClientJoinRandomGameEventHandler ClientJoinRandomGame;
        event ClientCreateAndJoinGameEventHandler ClientCreateAndJoinGame;

        // Game room as game master (player or spectator)
        event ClientStartGameEventHandler ClientStartGame;
        event ClientStopGameEventHandler ClientStopGame;
        event ClientPauseGameEventHandler ClientPauseGame;
        event ClientResumeGameEventHandler ClientResumeGame;
        event ClientChangeOptionsEventHandler ClientChangeOptions;
        event ClientVoteKickEventHandler ClientVoteKick;
        event ClientVoteKickResponseEventHandler ClientVoteKickAnswer;
        event ClientResetWinListEventHandler ClientResetWinList;

        // Game room as player or spectator
        event ClientLeaveGameEventHandler ClientLeaveGame;

        // Game room as player
        event ClientPlacePieceEventHandler ClientPlacePiece;
        event ClientModifyGridEventHandler ClientModifyGrid;
        event ClientUseSpecialEventHandler ClientUseSpecial;
        event ClientClearLinesEventHandler ClientClearLines;
        event ClientGameLostEventHandler ClientGameLost;
        event ClientFinishContinuousSpecialEventHandler ClientFinishContinuousSpecial;
        event ClientEarnAchievementEventHandler ClientEarnAchievement;

        // ------
        // Admin
        // Connect/Disconnect
        event AdminConnectEventHandler AdminConnect;
        event AdminDisconnectEventHandler AdminDisconnect;

        // Messaging
        event AdminSendPrivateAdminMessageEventHandler AdminSendPrivateAdminMessage;
        event AdminSendPrivateMessageEventHandler AdminSendPrivateMessage;
        event AdminSendBroadcastMessageEventHandler AdminSendBroadcastMessage;

        // Monitoring
        event AdminGetAdminListEventHandler AdminGetAdminList;
        event AdminGetClientListEventHandler AdminGetClientList;
        event AdminGetClientListInRoomEventHandler AdminGetClientListInRoom;
        event AdminGetRoomListEventHandler AdminGetRoomList;
        event AdminGetBannedListEventHandler AdminGetBannedList;

        // Kick/Ban
        event AdminKickEventHandler AdminKick;
        event AdminBanEventHandler AdminBan;

        // Server commands
        event AdminRestartServerEventHandler AdminRestartServer;

        //
        IClientManager ClientManager { get; }
        IGameRoomManager GameRoomManager { get; }
        IAdminManager AdminManager { get; }

        //
        void Start();
        void Stop();

        // Called when a client/admin/room is removed from client/admin/room manager
        void AddClient(IClient added);
        void AddAdmin(IAdmin added);
        void AddRoom(IGameRoom added);
        void RemoveClient(IClient removed);
        void RemoveAdmin(IAdmin removed);
        void RemoveGameRoom(IGameRoom removed);
    }
}
