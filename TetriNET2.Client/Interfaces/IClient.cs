using System.Collections.Generic;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Client.Interfaces
{
    public delegate void ConnectionLostEventHandler();

    public delegate void ConnectedEventHandler(ConnectResults result, Versioning serverVersion, ClientData client, List<GameData> games);
    public delegate void DisconnectedEventHandler();

    public delegate void ServerStoppedEventHandler();

    public delegate void GameListReceivedEventHandler(List<GameData> games);
    public delegate void ClientListReceivedEventHandler(List<ClientData> clients);
    public delegate void GameClientListReceivedEventHandler(List<ClientData> clients);

    public delegate void ClientConnectedEventHandler(ClientData client, string name, string team);
    public delegate void ClientDisconnectedEventHandler(ClientData client, LeaveReasons reason);
    public delegate void ClientGameCreatedEventHandler(ClientData client, GameData game);
    public delegate void ServerGameCreatedEventHandler(GameData game);
    public delegate void ServerGameDeletedEventHandler(GameData game);

    public delegate void ServerMessageReceivedEventHandler(string message);
    public delegate void BroadcastMessageReceivedEventHandler(ClientData client, string message);
    public delegate void PrivateMessageReceivedEventHandler(ClientData client, string message);
    public delegate void TeamChangedEventHandler(ClientData client, string team);
    public delegate void GameCreatedEventHandler(GameCreateResults result, GameData game);

    public delegate void GameJoinedEventHandler(GameJoinResults result, GameData game, bool isGameMaster);
    public delegate void GameLeftEventHandler();
    public delegate void ClientGameJoinedEventHandler(ClientData player, bool asSpectator);
    public delegate void ClientGameLeftEventHandler(ClientData player);
    public delegate void GameMasterModifiedEventHandler(ClientData player);
    public delegate void GameStartedEventHandler();
    public delegate void GamePausedEventHandler();
    public delegate void GameResumedEventHandler();
    public delegate void GameFinishedEventHandler(GameFinishedReasons reason, GameStatistics statistics);
    public delegate void WinListModifiedEventHandler(List<WinEntry> winEntries);
    public delegate void GameOptionsChangedEventHandler(GameOptions gameOptions);
    public delegate void VoteKickAskedEventHandler(ClientData sourceClient, ClientData targetClient, string reason);
    public delegate void AchievementEarnedEventHandler(ClientData player, int achievementId, string achievementTitle);

    public delegate void PiecePlacedEventHandler(int firstIndex, List<Pieces> nextPieces);
    public delegate void PlayerWonEventHandler(ClientData player);
    public delegate void PlayerLostEventHandler(ClientData player);
    public delegate void ServerLinesAddedEventHandler(int count);
    public delegate void PlayerLinesAddedEventHandler(ClientData player, int specialId, int count);
    public delegate void SpecialUsedEventHandler(ClientData player, ClientData target, int specialId, Specials special);
    public delegate void GridModifiedEventHandler(ClientData player, byte[] grid);
    public delegate void ContinuousSpecialFinishedEventHandler(ClientData player, Specials special);

    public interface IClient : ITetriNETClientCallback
    {
        string Name { get; }
        string Team { get; }
        Versioning Version { get; }

        // Following list are updated internally with ITetriNETClientCallback notifications
        IEnumerable<ClientData> Clients { get; }
        IEnumerable<ClientData> GameClients { get; }
        IEnumerable<GameData> Games { get; }

        //
        void SetVersion(int major, int minor);

        //
        event ConnectionLostEventHandler ConnectionLost;
         
        event ConnectedEventHandler Connected;
        event DisconnectedEventHandler Disconnected;
         
        event ServerStoppedEventHandler ServerStopped;
         
        event GameListReceivedEventHandler GameListReceived;
        event ClientListReceivedEventHandler ClientListReceived;
        event GameClientListReceivedEventHandler GameClientListReceived;
         
        event ClientConnectedEventHandler ClientConnected;
        event ClientDisconnectedEventHandler ClientDisconnected;
        event ClientGameCreatedEventHandler ClientGameCreated;
        event ServerGameCreatedEventHandler ServerGameCreated;
        event ServerGameDeletedEventHandler ServerGameDeleted;
         
        event ServerMessageReceivedEventHandler ServerMessageReceived;
        event BroadcastMessageReceivedEventHandler BroadcastMessageReceived;
        event PrivateMessageReceivedEventHandler PrivateMessageReceived;
        event TeamChangedEventHandler TeamChanged;
        event GameCreatedEventHandler GameCreated;
         
        event GameJoinedEventHandler GameJoined;
        event GameLeftEventHandler GameLeft;
        event ClientGameJoinedEventHandler ClientGameJoined;
        event ClientGameLeftEventHandler ClientGameLeft;
        event GameMasterModifiedEventHandler GameMasterModified;
        event GameStartedEventHandler GameStarted;
        event GamePausedEventHandler GamePaused;
        event GameResumedEventHandler GameResumed;
        event GameFinishedEventHandler GameFinished;
        event WinListModifiedEventHandler WinListModified;
        event GameOptionsChangedEventHandler GameOptionsChanged;
        event VoteKickAskedEventHandler VoteKickAsked;
        event AchievementEarnedEventHandler AchievementEarned;
         
        event PiecePlacedEventHandler PiecePlaced;
        event PlayerWonEventHandler PlayerWon;
        event PlayerLostEventHandler PlayerLost;
        event ServerLinesAddedEventHandler ServerLinesAdded;
        event PlayerLinesAddedEventHandler PlayerLinesAdded;
        event SpecialUsedEventHandler SpecialUsed;
        event GridModifiedEventHandler GridModified;
        event ContinuousSpecialFinishedEventHandler ContinuousSpecialFinished;

        // Connect/disconnect
        bool Connect(string address, string name, string team);
        bool Disconnect();

        // Messaging
        bool SendPrivateMessage(ClientData target, string message);
        bool SendBroadcastMessage(string message);

        // Wait
        bool ChangeTeam(string team);
        bool JoinGame(GameData game, string password, bool asSpectator);
        bool JoinRandomGame(bool asSpectator);
        bool CreateAndJoinGame(string name, string password, GameRules rule, bool asSpectator);
        bool GetGameList();
        bool GetClientList();

        // Game
        bool StartGame();
        bool StopGame();
        bool PauseGame();
        bool ResumeGame();
        bool ChangeOptions(GameOptions options);
        bool VoteKick(ClientData target, string reason);
        bool VoteKickResponse(bool accepted);
        bool ResetWinList();
        bool LeaveGame();
        bool GetGameClientList();

        // Game controller
        void Hold();
        void Drop();
        void MoveDown(bool automatic = false);
        void MoveLeft();
        void MoveRight();
        void RotateClockwise();
        void RotateCounterClockwise();
        void DiscardFirstSpecial();
        bool UseFirstSpecial(int targetId);

        // Achievement
        void ResetAchievements();
    }
}