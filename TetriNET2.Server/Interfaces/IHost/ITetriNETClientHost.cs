using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces.IHost
{
    // ------
    // Client
    // Connect/disconnect/keep alive
    public delegate void ClientConnectEventHandler(ITetriNETClientCallback callback, IAddress address, Versioning version, string name, string team);
    public delegate void ClientDisconnectEventHandler(IClient client);
    public delegate void ClientHeartbeatEventHandler(IClient client);

    // Wait+Game
    public delegate void ClientSendPrivateMessageEventHandler(IClient client, IClient target, string message);
    public delegate void ClientSendBroadcastMessageEventHandler(IClient client, string message);
    public delegate void ClientChangeTeamEventHandler(IClient client, string team);

    // Wait
    public delegate void ClientJoinGameEventHandler(IClient client, IGame game, string password, bool asSpectator);
    public delegate void ClientJoinRandomGameEventHandler(IClient client, bool asSpectator);
    public delegate void ClientCreateAndJoinGameEventHandler(IClient client, string name, string password, GameRules rule, bool asSpectator);
    public delegate void ClientGetGameListEventHandler(IClient client);
    public delegate void ClientGetClientListEventHandler(IClient client);

    // Game as game master (player or spectator)
    public delegate void ClientStartGameEventHandler(IClient client);
    public delegate void ClientStopGameEventHandler(IClient client);
    public delegate void ClientPauseGameEventHandler(IClient client);
    public delegate void ClientResumeGameEventHandler(IClient client);
    public delegate void ClientChangeOptionsEventHandler(IClient client, GameOptions options);
    public delegate void ClientResetWinListEventHandler(IClient client);
    public delegate void ClientVoteKickEventHandler(IClient client, IClient target, string reason);
    public delegate void ClientVoteKickResponseEventHandler(IClient client, bool accepted);

    // Game as player or spectator
    public delegate void ClientLeaveGameEventHandler(IClient client);
    public delegate void ClientGetGameClientListEventHandler(IClient client);

    // Game as player
    public delegate void ClientPlacePieceEventHandler(IClient client, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid);
    public delegate void ClientModifyGridEventHandler(IClient client, byte[] grid);
    public delegate void ClientUseSpecialEventHandler(IClient client, IClient target, Specials special);
    public delegate void ClientClearLinesEventHandler(IClient client, int count);
    public delegate void ClientGameLostEventHandler(IClient client);
    public delegate void ClientFinishContinuousSpecialEventHandler(IClient client, Specials special);
    public delegate void ClientEarnAchievementEventHandler(IClient client, int achievementId, string achievementTitle);

    public partial interface IHost : ITetriNETClient
    {
        // ------
        // Client
        // Connect/disconnect/keep alive
        event ClientConnectEventHandler HostClientConnect;
        event ClientDisconnectEventHandler HostClientDisconnect;
        event ClientHeartbeatEventHandler HostClientHeartbeat;

        // Wait+Game
        event ClientSendPrivateMessageEventHandler HostClientSendPrivateMessage;
        event ClientSendBroadcastMessageEventHandler HostClientSendBroadcastMessage;
        event ClientChangeTeamEventHandler HostClientChangeTeam;

        // Wait
        event ClientJoinGameEventHandler HostClientJoinGame;
        event ClientJoinRandomGameEventHandler HostClientJoinRandomGame;
        event ClientCreateAndJoinGameEventHandler HostClientCreateAndJoinGame;
        event ClientGetGameListEventHandler HostClientGetGameList;
        event ClientGetClientListEventHandler HostClientGetClientList;

        // Game as game master (player or spectator)
        event ClientStartGameEventHandler HostClientStartGame;
        event ClientStopGameEventHandler HostClientStopGame;
        event ClientPauseGameEventHandler HostClientPauseGame;
        event ClientResumeGameEventHandler HostClientResumeGame;
        event ClientChangeOptionsEventHandler HostClientChangeOptions;
        event ClientResetWinListEventHandler HostClientResetWinList;
        event ClientVoteKickEventHandler HostClientVoteKick;
        event ClientVoteKickResponseEventHandler HostClientVoteKickAnswer;

        // Game as player or spectator
        event ClientLeaveGameEventHandler HostClientLeaveGame;
        event ClientGetGameClientListEventHandler HostClientGetGameClientList;

        // Game as player
        event ClientPlacePieceEventHandler HostClientPlacePiece;
        event ClientModifyGridEventHandler HostClientModifyGrid;
        event ClientUseSpecialEventHandler HostClientUseSpecial;
        event ClientClearLinesEventHandler HostClientClearLines;
        event ClientGameLostEventHandler HostClientGameLost;
        event ClientFinishContinuousSpecialEventHandler HostClientFinishContinuousSpecial;
        event ClientEarnAchievementEventHandler HostClientEarnAchievement;
    }
}
