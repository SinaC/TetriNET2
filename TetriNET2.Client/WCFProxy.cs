using System;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET2.Client.Interfaces;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;

namespace TetriNET2.Client
{
    public class WCFProxy : IProxy
    {
        private DuplexChannelFactory<ITetriNETClient> _factory;
        private readonly ITetriNETClient _proxy;

        public WCFProxy(ITetriNETClientCallback callback, string address)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            if (address == null)
                throw new ArgumentNullException("address");

            LastActionToServer = DateTime.Now;

            // Get WCF endpoint
            EndpointAddress endpointAddress = new EndpointAddress(address);

            // Create WCF proxy from endpoint
            Log.Default.WriteLine(LogLevels.Debug, "Connecting to server:{0}", endpointAddress.Uri);
            Binding binding = new NetTcpBinding(SecurityMode.None);
            InstanceContext instanceContext = new InstanceContext(callback);
            _factory = new DuplexChannelFactory<ITetriNETClient>(instanceContext, binding, endpointAddress);
            _proxy = _factory.CreateChannel(instanceContext);
        }

        private void ExceptionFreeAction(Action action, [CallerMemberName]string actionName = null)
        {
            try
            {
                action();
                LastActionToServer = DateTime.Now;
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                ConnectionLost.Do(x => x());
                _factory.Do(x => x.Abort());
            }
        }

        #region IProxy

        public DateTime LastActionToServer { get; private set; } // used to check if heartbeat is needed

        public event ProxyClientConnectionLostEventHandler ConnectionLost;

        public bool Disconnect()
        {
            if (_factory == null)
                return false; // should connect first
            try
            {
                _factory.Close();
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Exception:{0}", ex);
                _factory.Abort();
            }
            _factory = null;
            return true;
        }

        #region ITetriNETClient

        public void ClientConnect(Versioning version, string name, string team)
        {
            ExceptionFreeAction(() => _proxy.ClientConnect(version, name, team));
        }

        public void ClientDisconnect()
        {
            ExceptionFreeAction(() => _proxy.ClientDisconnect());
        }

        public void ClientHeartbeat()
        {
            ExceptionFreeAction(() => _proxy.ClientHeartbeat());
        }

        public void ClientSendPrivateMessage(Guid targetId, string message)
        {
            ExceptionFreeAction(() => _proxy.ClientSendPrivateMessage(targetId, message));
        }

        public void ClientSendBroadcastMessage(string message)
        {
            ExceptionFreeAction(() => _proxy.ClientSendBroadcastMessage(message));
        }

        public void ClientChangeTeam(string team)
        {
            ExceptionFreeAction(() => _proxy.ClientChangeTeam(team));
        }

        public void ClientJoinGame(Guid gameId, string password, bool asSpectator)
        {
            ExceptionFreeAction(() => _proxy.ClientJoinGame(gameId, password, asSpectator));
        }

        public void ClientJoinRandomGame(bool asSpectator)
        {
            ExceptionFreeAction(() => _proxy.ClientJoinRandomGame(asSpectator));
        }

        public void ClientCreateAndJoinGame(string name, string password, GameRules rule, bool asSpectator)
        {
            ExceptionFreeAction(() => _proxy.ClientCreateAndJoinGame(name, password, rule, asSpectator));
        }

        public void ClientGetGameList()
        {
            ExceptionFreeAction(() => _proxy.ClientGetGameList());
        }

        public void ClientGetClientList()
        {
            ExceptionFreeAction(() => _proxy.ClientGetClientList());
        }

        public void ClientStartGame()
        {
            ExceptionFreeAction(() => _proxy.ClientStartGame());
        }

        public void ClientStopGame()
        {
            ExceptionFreeAction(() => _proxy.ClientStopGame());
        }

        public void ClientPauseGame()
        {
            ExceptionFreeAction(() => _proxy.ClientPauseGame());
        }

        public void ClientResumeGame()
        {
            ExceptionFreeAction(() => _proxy.ClientResumeGame());
        }

        public void ClientChangeOptions(GameOptions options)
        {
            ExceptionFreeAction(() => _proxy.ClientChangeOptions(options));
        }

        public void ClientVoteKick(Guid targetId, string reason)
        {
            ExceptionFreeAction(() => _proxy.ClientVoteKick(targetId, reason));
        }

        public void ClientVoteKickResponse(bool accepted)
        {
            ExceptionFreeAction(() => _proxy.ClientVoteKickResponse(accepted));
        }

        public void ClientResetWinList()
        {
            ExceptionFreeAction(() => _proxy.ClientResetWinList());
        }

        public void ClientLeaveGame()
        {
            ExceptionFreeAction(() => _proxy.ClientLeaveGame());
        }

        public void ClientGetGameClientList()
        {
            ExceptionFreeAction(() => _proxy.ClientGetGameClientList());
        }

        public void ClientPlacePiece(int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            ExceptionFreeAction(() => _proxy.ClientPlacePiece(pieceIndex, highestIndex, piece, orientation, posX, posX, grid));
        }

        public void ClientModifyGrid(byte[] grid)
        {
            ExceptionFreeAction(() => _proxy.ClientModifyGrid(grid));
        }

        public void ClientUseSpecial(Guid targetId, Specials special)
        {
            ExceptionFreeAction(() => _proxy.ClientUseSpecial(targetId, special));
        }

        public void ClientClearLines(int count)
        {
            ExceptionFreeAction(() => _proxy.ClientClearLines(count));
        }

        public void ClientGameLost()
        {
            ExceptionFreeAction(() => _proxy.ClientGameLost());
        }

        public void ClientFinishContinuousSpecial(Specials special)
        {
            ExceptionFreeAction(() => _proxy.ClientFinishContinuousSpecial(special));
        }

        public void ClientEarnAchievement(int achievementId, string achievementTitle)
        {
            ExceptionFreeAction(() => _proxy.ClientEarnAchievement(achievementId, achievementTitle));
        }

        #endregion

        #endregion
    }
}
