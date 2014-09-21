using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces.IHost
{
    // ------
    // Client
    // Connect/disconnect/keep alive
    public delegate void HostClientConnectEventHandler(ITetriNETCallback callback, IPAddress address, Versioning version, string name, string team);
    public delegate void HostClientDisconnectEventHandler(IClient client);
    public delegate void HostClientHeartbeatEventHandler(IClient client);

    // Wait+Game room
    public delegate void HostClientSendPrivateMessageEventHandler(IClient client, IClient target, string message);
    public delegate void HostClientSendBroadcastMessageEventHandler(IClient client, string message);
    public delegate void HostClientChangeTeamEventHandler(IClient client, string team);

    // Wait room
    public delegate void HostClientJoinGameEventHandler(IClient client, IGameRoom game, string password, bool asSpectator);
    public delegate void HostClientJoinRandomGameEventHandler(IClient client, bool asSpectator);
    public delegate void HostClientCreateAndJoinGameEventHandler(IClient client, string name, string password, GameRules rule, bool asSpectator);

    // Game room as game master (player or spectator)
    public delegate void HostClientStartGameEventHandler(IClient client);
    public delegate void HostClientStopGameEventHandler(IClient client);
    public delegate void HostClientPauseGameEventHandler(IClient client);
    public delegate void HostClientResumeGameEventHandler(IClient client);
    public delegate void HostClientChangeOptionsEventHandler(IClient client, GameOptions options);
    public delegate void HostClientVoteKickEventHandler(IClient client, IClient target, string reason);
    public delegate void HostClientVoteKickResponseEventHandler(IClient client, bool accepted);
    public delegate void HostClientResetWinListEventHandler(IClient client);

    // Game room as player or spectator
    public delegate void HostClientLeaveGameEventHandler(IClient client);

    // Game room as player
    public delegate void HostClientPlacePieceEventHandler(IClient client, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid);
    public delegate void HostClientModifyGridEventHandler(IClient client, byte[] grid);
    public delegate void HostClientUseSpecialEventHandler(IClient client, IClient target, Specials special);
    public delegate void HostClientClearLinesEventHandler(IClient client, int count);
    public delegate void HostClientGameLostEventHandler(IClient client);
    public delegate void HostClientFinishContinuousSpecialEventHandler(IClient client, Specials special);
    public delegate void HostClientEarnAchievementEventHandler(IClient client, int achievementId, string achievementTitle);

    public partial interface IHost : ITetriNET
    {
        // ------
        // Client
        // Connect/disconnect/keep alive
        event HostClientConnectEventHandler HostClientConnect;
        event HostClientDisconnectEventHandler HostClientDisconnect;
        event HostClientHeartbeatEventHandler HostClientHeartbeat;

        // Wait+Game room
        event HostClientSendPrivateMessageEventHandler HostClientSendPrivateMessage;
        event HostClientSendBroadcastMessageEventHandler HostClientSendBroadcastMessage;
        event HostClientChangeTeamEventHandler HostClientChangeTeam;

        // Wait room
        event HostClientJoinGameEventHandler HostClientJoinGame;
        event HostClientJoinRandomGameEventHandler HostClientJoinRandomGame;
        event HostClientCreateAndJoinGameEventHandler HostClientCreateAndJoinGame;

        // Game room as game master (player or spectator)
        event HostClientStartGameEventHandler HostClientStartGame;
        event HostClientStopGameEventHandler HostClientStopGame;
        event HostClientPauseGameEventHandler HostClientPauseGame;
        event HostClientResumeGameEventHandler HostClientResumeGame;
        event HostClientChangeOptionsEventHandler HostClientChangeOptions;
        event HostClientVoteKickEventHandler HostClientVoteKick;
        event HostClientVoteKickResponseEventHandler HostClientVoteKickAnswer;
        event HostClientResetWinListEventHandler HostClientResetWinList;

        // Game room as player or spectator
        event HostClientLeaveGameEventHandler HostClientLeaveGame;

        // Game room as player
        event HostClientPlacePieceEventHandler HostClientPlacePiece;
        event HostClientModifyGridEventHandler HostClientModifyGrid;
        event HostClientUseSpecialEventHandler HostClientUseSpecial;
        event HostClientClearLinesEventHandler HostClientClearLines;
        event HostClientGameLostEventHandler HostClientGameLost;
        event HostClientFinishContinuousSpecialEventHandler HostClientFinishContinuousSpecial;
        event HostClientEarnAchievementEventHandler HostClientEarnAchievement;
    }
}
