using System;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Common.Contracts
{
    public interface ITetriNET
    {
        // Connect/disconnect/keep alive
        void ClientConnect(ITetriNETCallback callback, Versioning version, string name, string team);
        void ClientDisconnect(ITetriNETCallback callback);
        void ClientHeartbeat(ITetriNETCallback callback);

        // Wait+Game room
        void ClientSendPrivateMessage(ITetriNETCallback callback, Guid targetId, string message);
        void ClientSendBroadcastMessage(ITetriNETCallback callback, string message);
        void ClientChangeTeam(ITetriNETCallback callback, string team);

        // Wait room
        void ClientJoinGame(ITetriNETCallback callback, Guid gameId, string password, bool asSpectator);
        void ClientJoinRandomGame(ITetriNETCallback callback, bool asSpectator);
        void ClientCreateAndJoinGame(ITetriNETCallback callback, string name, string password, GameRules rule, bool asSpectator);

        // Game room as game master (player or spectator)
        void ClientStartGame(ITetriNETCallback callback);
        void ClientStopGame(ITetriNETCallback callback);
        void ClientPauseGame(ITetriNETCallback callback);
        void ClientResumeGame(ITetriNETCallback callback);
        void ClientChangeOptions(ITetriNETCallback callback, GameOptions options);
        void ClientVoteKick(ITetriNETCallback callback, Guid targetId, string reason);
        void ClientVoteKickResponse(ITetriNETCallback callback, bool accepted);
        void ClientResetWinList(ITetriNETCallback callback);

        // Game room as player or spectator
        void ClientLeaveGame(ITetriNETCallback callback);

        // Game room as player
        void ClientPlacePiece(ITetriNETCallback callback, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid);
        void ClientModifyGrid(ITetriNETCallback callback, byte[] grid);
        void ClientUseSpecial(ITetriNETCallback callback, Guid targetId, Specials special);
        void ClientClearLines(ITetriNETCallback callback, int count);
        void ClientGameLost(ITetriNETCallback callback);
        void ClientFinishContinuousSpecial(ITetriNETCallback callback, Specials special);
        void ClientEarnAchievement(ITetriNETCallback callback, int achievementId, string achievementTitle);
    }
}
