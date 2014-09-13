using System;
using System.Collections.Generic;
using System.Threading;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces
{
    public interface IGameRoom
    {
        Guid Id { get; }
        string Name { get; }
        DateTime CreationTime { get; }
        string Password { get; }
        GameRoomStates State { get; }

        int MaxPlayers { get; }
        int MaxSpectators { get; }
        int ClientCount { get; }
        int PlayerCount { get; }
        int SpectatorCount { get; }
        object LockObject { get; }

        DateTime GameStartTime { get; }
        GameOptions Options { get; }
        GameRules Rule { get; }
        
        IEnumerable<IClient> Clients { get; }
        IEnumerable<IClient> Players { get; }
        IEnumerable<IClient> Spectators { get; }

        bool Join(IClient client, bool asSpectator);
        bool Leave(IClient client);
        void Clear();

        void Start(CancellationTokenSource cancellationTokenSource);
        void Stop();

        void ChangeOptions(GameOptions options);
        void ResetWinList();

        void PlacePiece(IClient client, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid);
        void ModifyGrid(IClient client, byte[] grid);
        void UseSpecial(IClient client, IClient target, Specials special);
        void ClearLines(IClient client, int count);
        void GameLost(IClient client);
        void FinishContinuousSpecial(IClient client, Specials special);
        void EarnAchievement(IClient client, int achievementId, string achievementTitle);

        void StartGame();
        void StopGame();
        void PauseGame();
        void ResumeGame();
    }
}
