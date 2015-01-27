using System;
using System.Collections.Generic;
using System.Threading;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces
{
    public interface IGame
    {
        Guid Id { get; }
        string Name { get; }
        DateTime CreationTime { get; }
        string Password { get; }
        GameStates State { get; }

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

        bool Start(CancellationTokenSource cancellationTokenSource);
        bool Stop();

        bool Join(IClient client, bool asSpectator);
        bool Leave(IClient client);
        void Clear();

        bool VoteKick(IClient client, IClient target, string reason);
        bool VoteKickAnswer(IClient client, bool accepted);

        bool PlacePiece(IClient client, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid);
        bool ModifyGrid(IClient client, byte[] grid);
        bool UseSpecial(IClient client, IClient target, Specials special);
        bool ClearLines(IClient client, int count);
        bool GameLost(IClient client);
        bool FinishContinuousSpecial(IClient client, Specials special);
        bool EarnAchievement(IClient client, int achievementId, string achievementTitle);

        // Following methods may be called by a client (game master) or by server (client is null in this case)
        bool StartGame(IClient client);
        bool StopGame(IClient client);
        bool PauseGame(IClient client);
        bool ResumeGame(IClient client);
        bool ChangeOptions(IClient client, GameOptions options);
        bool ResetWinList(IClient client);
    }
}
