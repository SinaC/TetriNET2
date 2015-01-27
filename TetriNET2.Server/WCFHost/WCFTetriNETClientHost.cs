using System;
using System.ServiceModel;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;

namespace TetriNET2.Server.WCFHost
{
    public sealed partial class WCFHost : IHost
    {
        private ITetriNETClientCallback ClientCallback
        {
            get { return OperationContext.Current.GetCallbackChannel<ITetriNETClientCallback>(); }
        }

        #region ITetriNETClientHost

        public event HostClientConnectEventHandler HostClientConnect;
        public event HostClientDisconnectEventHandler HostClientDisconnect;
        public event HostClientHeartbeatEventHandler HostClientHeartbeat;

        public event HostClientSendPrivateMessageEventHandler HostClientSendPrivateMessage;
        public event HostClientSendBroadcastMessageEventHandler HostClientSendBroadcastMessage;

        public event HostClientChangeTeamEventHandler HostClientChangeTeam;
        public event HostClientJoinGameEventHandler HostClientJoinGame;
        public event HostClientJoinRandomGameEventHandler HostClientJoinRandomGame;
        public event HostClientCreateAndJoinGameEventHandler HostClientCreateAndJoinGame;
        public event HostClientGetGameListEventHandler HostClientGetGameList;
        public event HostClientGetClientListEventHandler HostClientGetClientList;

        public event HostClientStartGameEventHandler HostClientStartGame;
        public event HostClientStopGameEventHandler HostClientStopGame;
        public event HostClientPauseGameEventHandler HostClientPauseGame;
        public event HostClientResumeGameEventHandler HostClientResumeGame;
        public event HostClientChangeOptionsEventHandler HostClientChangeOptions;
        public event HostClientVoteKickEventHandler HostClientVoteKick;
        public event HostClientVoteKickResponseEventHandler HostClientVoteKickAnswer;
        public event HostClientResetWinListEventHandler HostClientResetWinList;

        public event HostClientLeaveGameEventHandler HostClientLeaveGame;
        public event HostClientGetGameClientListEventHandler HostClientGetGameClientList;

        public event HostClientPlacePieceEventHandler HostClientPlacePiece;
        public event HostClientModifyGridEventHandler HostClientModifyGrid;
        public event HostClientUseSpecialEventHandler HostClientUseSpecial;
        public event HostClientClearLinesEventHandler HostClientClearLines;
        public event HostClientGameLostEventHandler HostClientGameLost;
        public event HostClientFinishContinuousSpecialEventHandler HostClientFinishContinuousSpecial;
        public event HostClientEarnAchievementEventHandler HostClientEarnAchievement;

        #region ITetriNET

        public void ClientConnect(Versioning version, string name, string team)
        {
            HostClientConnect.Do(x => x(ClientCallback, Address, version, name, team));
        }

        public void ClientDisconnect()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientDisconnect.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientDisconnect from unknown client");
        }

        public void ClientHeartbeat()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientHeartbeat.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientHeartbeat from unknown client");
        }

        public void ClientSendPrivateMessage(Guid targetId, string message)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
            {
                IClient target = ClientManager[targetId];
                if (target != null)
                    HostClientSendPrivateMessage.Do(x => x(client, target, message));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "ClientSendPrivateMessage to unknown client");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientSendPrivateMessage from unknown client");
        }

        public void ClientSendBroadcastMessage(string message)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientSendBroadcastMessage.Do(x => x(client, message));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientSendBroadcastMessage from unknown client");
        }

        public void ClientChangeTeam(string team)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientChangeTeam.Do(x => x(client, team));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientChangeTeam from unknown client");
        }

        public void ClientJoinGame(Guid gameId, string password, bool asSpectator)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
            {
                IGame game = GameManager[gameId];
                if (game != null)
                    HostClientJoinGame.Do(x => x(client, game, password, asSpectator));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "ClientJoinGame to unknown game");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientJoinGame from unknown client");
        }

        public void ClientJoinRandomGame(bool asSpectator)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientJoinRandomGame.Do(x => x(client, asSpectator));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientJoinRandomGame from unknown client");
        }

        public void ClientCreateAndJoinGame(string name, string password, GameRules rule, bool asSpectator)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientCreateAndJoinGame.Do(x => x(client, name, password, rule, asSpectator));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientCreateAndJoinGame from unknown client");
        }

        public void ClientGetGameList()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientGetGameList.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientGetGameList from unknown client");
        }

        public void ClientGetClientList()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientGetClientList.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientGetClientList from unknown client");
        }

        public void ClientStartGame()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientStartGame.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientStartGame from unknown client");
        }

        public void ClientStopGame()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientStopGame.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientStopGame from unknown client");
        }

        public void ClientPauseGame()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientPauseGame.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientPauseGame from unknown client");
        }

        public void ClientResumeGame()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientResumeGame.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientResumeGame from unknown client");
        }

        public void ClientChangeOptions(GameOptions options)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientChangeOptions.Do(x => x(client, options));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientChangeOptions from unknown client");
        }

        public void ClientVoteKick(Guid targetId, string reason)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
            {
                IClient target = ClientManager[targetId];
                if (target != null)
                    HostClientVoteKick.Do(x => x(client, target, reason));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "ClientVoteKick to unknown client");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientVoteKick from unknown client");
        }

        public void ClientVoteKickResponse(bool accepted)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientVoteKickAnswer.Do(x => x(client, accepted));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientVoteKickResponse from unknown client");
        }

        public void ClientResetWinList()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientResetWinList.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientResetWinList from unknown client");
        }

        public void ClientLeaveGame()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientLeaveGame.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientLeaveGame from unknown client");
        }

        public void ClientGetGameClientList()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientGetGameClientList.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientGetGameClientList from unknown client");
        }

        public void ClientPlacePiece(int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientPlacePiece.Do(x => x(client, pieceIndex, highestIndex, piece, orientation, posX, posY, grid));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientPlacePiece from unknown client");
        }

        public void ClientModifyGrid(byte[] grid)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientModifyGrid.Do(x => x(client, grid));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientModifyGrid from unknown client");
        }

        public void ClientUseSpecial(Guid targetId, Specials special)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
            {
                IClient target = ClientManager[targetId];
                if (target != null)
                    HostClientUseSpecial.Do(x => x(client, target, special));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "ClientUseSpecial to unknown client");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientUseSpecial from unknown client");
        }

        public void ClientClearLines(int count)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientClearLines.Do(x => x(client, count));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientClearLines from unknown client");
        }

        public void ClientGameLost()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientGameLost.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientGameLost from unknown client");
        }

        public void ClientFinishContinuousSpecial(Specials special)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientFinishContinuousSpecial.Do(x => x(client, special));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientFinishContinuousSpecial from unknown client");
        }

        public void ClientEarnAchievement(int achievementId, string achievementTitle)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null)
                HostClientEarnAchievement.Do(x => x(client, achievementId, achievementTitle));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientEarnAchievement from unknown client");
        }

        #endregion

        #endregion
    }
}
