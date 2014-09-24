using System;
using System.Net;
using System.ServiceModel;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Common.Contracts.WCF
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ITetriNETCallback))]
    public interface IWCFTetriNET
    {
        // Connect/disconnect/keep alive
        [OperationContract(IsOneWay = true)]
        void ClientConnect(Versioning version, string name, string team);
        [OperationContract(IsOneWay = true)]
        void ClientDisconnect();
        [OperationContract(IsOneWay = true)]
        void ClientHeartbeat();

        // Wait+Game room
        [OperationContract(IsOneWay = true)]
        void ClientSendPrivateMessage(Guid targetId, string message);
        [OperationContract(IsOneWay = true)]
        void ClientSendBroadcastMessage(string message);
        [OperationContract(IsOneWay = true)]
        void ClientChangeTeam(string team);

        // Wait room
        [OperationContract(IsOneWay = true)]
        void ClientJoinGame(Guid gameId, string password, bool asSpectator);
        [OperationContract(IsOneWay = true)]
        void ClientJoinRandomGame(bool asSpectator);
        [OperationContract(IsOneWay = true)]
        void ClientCreateAndJoinGame(string name, string password, GameRules rule, bool asSpectator);
        [OperationContract(IsOneWay = true)]
        void ClientGetRoomList();
        [OperationContract(IsOneWay = true)]
        void ClientGetClientList();

        // Game room as game master (player or spectator)
        [OperationContract(IsOneWay = true)]
        void ClientStartGame();
        [OperationContract(IsOneWay = true)]
        void ClientStopGame();
        [OperationContract(IsOneWay = true)]
        void ClientPauseGame();
        [OperationContract(IsOneWay = true)]
        void ClientResumeGame();
        [OperationContract(IsOneWay = true)]
        void ClientChangeOptions(GameOptions options);
        [OperationContract(IsOneWay = true)]
        void ClientVoteKick(Guid targetId, string reason);
        [OperationContract(IsOneWay = true)]
        void ClientVoteKickResponse(bool accepted);
        [OperationContract(IsOneWay = true)]
        void ClientResetWinList();

        // Game room as player or spectator
        [OperationContract(IsOneWay = true)]
        void ClientLeaveGame();
        [OperationContract(IsOneWay = true)]
        void ClientGetGameClientList();

        // Game room as player
        [OperationContract(IsOneWay = true)]
        void ClientPlacePiece(int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid);
        [OperationContract(IsOneWay = true)]
        void ClientModifyGrid(byte[] grid);
        [OperationContract(IsOneWay = true)]
        void ClientUseSpecial(Guid targetId, Specials special);
        [OperationContract(IsOneWay = true)]
        void ClientClearLines(int count);
        [OperationContract(IsOneWay = true)]
        void ClientGameLost();
        [OperationContract(IsOneWay = true)]
        void ClientFinishContinuousSpecial(Specials special);
        [OperationContract(IsOneWay = true)]
        void ClientEarnAchievement(int achievementId, string achievementTitle);
    }
}
