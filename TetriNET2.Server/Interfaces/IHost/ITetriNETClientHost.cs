using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces.IHost
{
    // ------
    // Client
    // Connect/disconnect/keep alive
    public delegate void HostClientConnectEventHandler(ITetriNETClientCallback callback, IPAddress address, Versioning version, string name, string team);
    public delegate void HostClientDisconnectEventHandler(IClient client);
    public delegate void HostClientHeartbeatEventHandler(IClient client);

    // Wait+Game
    public delegate void HostClientSendPrivateMessageEventHandler(IClient client, IClient target, string message);
    public delegate void HostClientSendBroadcastMessageEventHandler(IClient client, string message);
    public delegate void HostClientChangeTeamEventHandler(IClient client, string team);

    // Wait
    public delegate void HostClientJoinGameEventHandler(IClient client, IGame game, string password, bool asSpectator);
    public delegate void HostClientJoinRandomGameEventHandler(IClient client, bool asSpectator);
    public delegate void HostClientCreateAndJoinGameEventHandler(IClient client, string name, string password, GameRules rule, bool asSpectator);
    public delegate void HostClientGetGameListEventHandler(IClient client);
    public delegate void HostClientGetClientListEventHandler(IClient client);

    // Game as game master (player or spectator)
    public delegate void HostClientStartGameEventHandler(IClient client);
    public delegate void HostClientStopGameEventHandler(IClient client);
    public delegate void HostClientPauseGameEventHandler(IClient client);
    public delegate void HostClientResumeGameEventHandler(IClient client);
    public delegate void HostClientChangeOptionsEventHandler(IClient client, GameOptions options);
    public delegate void HostClientResetWinListEventHandler(IClient client);
    public delegate void HostClientVoteKickEventHandler(IClient client, IClient target, string reason);
    public delegate void HostClientVoteKickResponseEventHandler(IClient client, bool accepted);

    // Game as player or spectator
    public delegate void HostClientLeaveGameEventHandler(IClient client);
    public delegate void HostClientGetGameClientListEventHandler(IClient client);

    // Game as player
    public delegate void HostClientPlacePieceEventHandler(IClient client, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid);
    public delegate void HostClientModifyGridEventHandler(IClient client, byte[] grid);
    public delegate void HostClientUseSpecialEventHandler(IClient client, IClient target, Specials special);
    public delegate void HostClientClearLinesEventHandler(IClient client, int count);
    public delegate void HostClientGameLostEventHandler(IClient client);
    public delegate void HostClientFinishContinuousSpecialEventHandler(IClient client, Specials special);
    public delegate void HostClientEarnAchievementEventHandler(IClient client, int achievementId, string achievementTitle);

    public partial interface IHost : ITetriNETClient
    {
        // ------
        // Client
        // Connect/disconnect/keep alive
        event HostClientConnectEventHandler HostClientConnect;
        event HostClientDisconnectEventHandler HostClientDisconnect;
        event HostClientHeartbeatEventHandler HostClientHeartbeat;

        // Wait+Game
        event HostClientSendPrivateMessageEventHandler HostClientSendPrivateMessage;
        event HostClientSendBroadcastMessageEventHandler HostClientSendBroadcastMessage;
        event HostClientChangeTeamEventHandler HostClientChangeTeam;

        // Wait
        event HostClientJoinGameEventHandler HostClientJoinGame;
        event HostClientJoinRandomGameEventHandler HostClientJoinRandomGame;
        event HostClientCreateAndJoinGameEventHandler HostClientCreateAndJoinGame;
        event HostClientGetGameListEventHandler HostClientGetGameList;
        event HostClientGetClientListEventHandler HostClientGetClientList;

        // Game as game master (player or spectator)
        event HostClientStartGameEventHandler HostClientStartGame;
        event HostClientStopGameEventHandler HostClientStopGame;
        event HostClientPauseGameEventHandler HostClientPauseGame;
        event HostClientResumeGameEventHandler HostClientResumeGame;
        event HostClientChangeOptionsEventHandler HostClientChangeOptions;
        event HostClientResetWinListEventHandler HostClientResetWinList;
        event HostClientVoteKickEventHandler HostClientVoteKick;
        event HostClientVoteKickResponseEventHandler HostClientVoteKickAnswer;

        // Game as player or spectator
        event HostClientLeaveGameEventHandler HostClientLeaveGame;
        event HostClientGetGameClientListEventHandler HostClientGetGameClientList;

        // Game as player
        event HostClientPlacePieceEventHandler HostClientPlacePiece;
        event HostClientModifyGridEventHandler HostClientModifyGrid;
        event HostClientUseSpecialEventHandler HostClientUseSpecial;
        event HostClientClearLinesEventHandler HostClientClearLines;
        event HostClientGameLostEventHandler HostClientGameLost;
        event HostClientFinishContinuousSpecialEventHandler HostClientFinishContinuousSpecial;
        event HostClientEarnAchievementEventHandler HostClientEarnAchievement;
    }
}
