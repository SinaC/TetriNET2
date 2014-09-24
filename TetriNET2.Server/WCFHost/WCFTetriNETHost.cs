using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.Contracts.WCF;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;

namespace TetriNET2.Server.WCFHost
{
    public partial class WCFHost : IHost
    {
        [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
        public sealed class WCFClientServiceHost : IWCFTetriNET, IDisposable
        {
            private ServiceHost _serviceHost;
            private readonly IHost _host;

            public int Port { get; set; }

            public WCFClientServiceHost(IHost host)
            {
                if (host == null)
                    throw new ArgumentNullException("host");
                _host = host;
            }

            public void Start()
            {
                Uri baseAddress = new Uri(String.Format("net.tcp://localhost:{0}", Port));

                _serviceHost = new ServiceHost(this, baseAddress);
                _serviceHost.AddServiceEndpoint(typeof(IWCFTetriNET), new NetTcpBinding(SecurityMode.None), "/TetriNET2");
                _serviceHost.Description.Behaviors.Add(new IPFilterServiceBehavior(_host.BanManager, _host.ClientManager));
                _serviceHost.Open();

                Log.Default.WriteLine(LogLevels.Info, "WCF Client Host opened on {0}", baseAddress);

                foreach (var endpt in _serviceHost.Description.Endpoints)
                {
                    Log.Default.WriteLine(LogLevels.Debug, "Enpoint address:\t{0}", endpt.Address);
                    Log.Default.WriteLine(LogLevels.Debug, "Enpoint binding:\t{0}", endpt.Binding);
                    Log.Default.WriteLine(LogLevels.Debug, "Enpoint contract:\t{0}", endpt.Contract.ContractType.Name);
                }
            }

            public void Stop()
            {
                // Close service host
                _serviceHost.Do(x => x.Close());
            }

            #region IWCFTetriNET

            public void ClientConnect(Versioning version, string name, string team)
            {
                _host.ClientConnect(Callback, Address, version, name, team);
            }

            public void ClientDisconnect()
            {
                _host.ClientDisconnect(Callback);
            }

            public void ClientHeartbeat()
            {
                _host.ClientHeartbeat(Callback);
            }

            public void ClientSendPrivateMessage(Guid targetId, string message)
            {
                _host.ClientSendPrivateMessage(Callback, targetId, message);
            }

            public void ClientSendBroadcastMessage(string message)
            {
                _host.ClientSendBroadcastMessage(Callback, message);
            }

            public void ClientChangeTeam(string team)
            {
                _host.ClientChangeTeam(Callback, team);
            }

            public void ClientJoinGame(Guid gameId, string password, bool asSpectator)
            {
                _host.ClientJoinGame(Callback, gameId, password, asSpectator);
            }

            public void ClientJoinRandomGame(bool asSpectator)
            {
                _host.ClientJoinRandomGame(Callback, asSpectator);
            }

            public void ClientCreateAndJoinGame(string name, string password, GameRules rule, bool asSpectator)
            {
                _host.ClientCreateAndJoinGame(Callback, name, password, rule, asSpectator);
            }

            public void ClientGetRoomList()
            {
                _host.ClientGetRoomList(Callback);
            }

            public void ClientStartGame()
            {
                _host.ClientStartGame(Callback);
            }

            public void ClientStopGame()
            {
                _host.ClientStopGame(Callback);
            }

            public void ClientPauseGame()
            {
                _host.ClientPauseGame(Callback);
            }

            public void ClientResumeGame()
            {
                _host.ClientResumeGame(Callback);
            }

            public void ClientChangeOptions(GameOptions options)
            {
                _host.ClientChangeOptions(Callback, options);
            }

            public void ClientVoteKick(Guid targetId, string reason)
            {
                _host.ClientVoteKick(Callback, targetId, reason);
            }

            public void ClientVoteKickResponse(bool accepted)
            {
                _host.ClientVoteKickResponse(Callback, accepted);
            }

            public void ClientResetWinList()
            {
                _host.ClientResetWinList(Callback);
            }

            public void ClientLeaveGame()
            {
                _host.ClientLeaveGame(Callback);
            }

            public void ClientPlacePiece(int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
            {
                _host.ClientPlacePiece(Callback, pieceIndex, highestIndex, piece, orientation, posX, posY, grid);
            }

            public void ClientModifyGrid(byte[] grid)
            {
                _host.ClientModifyGrid(Callback, grid);
            }

            public void ClientUseSpecial(Guid targetId, Specials special)
            {
                _host.ClientUseSpecial(Callback, targetId, special);
            }

            public void ClientClearLines(int count)
            {
                _host.ClientClearLines(Callback, count);
            }

            public void ClientGameLost()
            {
                _host.ClientGameLost(Callback);
            }

            public void ClientFinishContinuousSpecial(Specials special)
            {
                _host.ClientFinishContinuousSpecial(Callback, special);
            }

            public void ClientEarnAchievement(int achievementId, string achievementTitle)
            {
                _host.ClientEarnAchievement(Callback, achievementId, achievementTitle);
            }

            #endregion

            #region IDisposable

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_serviceHost != null)
                        _serviceHost.Close();
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            #endregion

            private ITetriNETCallback Callback
            {
                get
                {
                    return OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
                }
            }

            private IPAddress Address
            {
                get
                {
                    MessageProperties messageProperties = OperationContext.Current.IncomingMessageProperties;
                    RemoteEndpointMessageProperty endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                    if (endpointProperty != null)
                        return IPAddress.Parse(endpointProperty.Address);
                    return null;
                }
            }
        }

        #region ITetriNETHost

        public event HostClientConnectEventHandler HostClientConnect;
        public event HostClientDisconnectEventHandler HostClientDisconnect;
        public event HostClientHeartbeatEventHandler HostClientHeartbeat;

        public event HostClientSendPrivateMessageEventHandler HostClientSendPrivateMessage;
        public event HostClientSendBroadcastMessageEventHandler HostClientSendBroadcastMessage;

        public event HostClientChangeTeamEventHandler HostClientChangeTeam;
        public event HostClientJoinGameEventHandler HostClientJoinGame;
        public event HostClientJoinRandomGameEventHandler HostClientJoinRandomGame;
        public event HostClientCreateAndJoinGameEventHandler HostClientCreateAndJoinGame;
        public event HostClientGetRoomListEventHandler HostClientGetRoomList;

        public event HostClientStartGameEventHandler HostClientStartGame;
        public event HostClientStopGameEventHandler HostClientStopGame;
        public event HostClientPauseGameEventHandler HostClientPauseGame;
        public event HostClientResumeGameEventHandler HostClientResumeGame;
        public event HostClientChangeOptionsEventHandler HostClientChangeOptions;
        public event HostClientVoteKickEventHandler HostClientVoteKick;
        public event HostClientVoteKickResponseEventHandler HostClientVoteKickAnswer;
        public event HostClientResetWinListEventHandler HostClientResetWinList;

        public event HostClientLeaveGameEventHandler HostClientLeaveGame;

        public event HostClientPlacePieceEventHandler HostClientPlacePiece;
        public event HostClientModifyGridEventHandler HostClientModifyGrid;
        public event HostClientUseSpecialEventHandler HostClientUseSpecial;
        public event HostClientClearLinesEventHandler HostClientClearLines;
        public event HostClientGameLostEventHandler HostClientGameLost;
        public event HostClientFinishContinuousSpecialEventHandler HostClientFinishContinuousSpecial;
        public event HostClientEarnAchievementEventHandler HostClientEarnAchievement;

        #region ITetriNET

        public void ClientConnect(ITetriNETCallback callback, IPAddress address, Versioning version, string name, string team)
        {
            HostClientConnect.Do(x => x(callback, address, version, name, team));
        }

        public void ClientDisconnect(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientDisconnect.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientDisconnect from unknown client");
        }

        public void ClientHeartbeat(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientHeartbeat.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientHeartbeat from unknown client");
        }

        public void ClientSendPrivateMessage(ITetriNETCallback callback, Guid targetId, string message)
        {
            IClient client = ClientManager[callback];
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

        public void ClientSendBroadcastMessage(ITetriNETCallback callback, string message)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientSendBroadcastMessage.Do(x => x(client, message));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientSendBroadcastMessage from unknown client");
        }

        public void ClientChangeTeam(ITetriNETCallback callback, string team)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientChangeTeam.Do(x => x(client, team));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientChangeTeam from unknown client");
        }

        public void ClientJoinGame(ITetriNETCallback callback, Guid gameId, string password, bool asSpectator)
        {
            IClient client = ClientManager[callback];
            if (client != null)
            {
                IGameRoom game = GameRoomManager[gameId];
                if (game != null)
                    HostClientJoinGame.Do(x => x(client, game, password, asSpectator));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "ClientJoinGame to unknown game");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientJoinGame from unknown client");
        }

        public void ClientJoinRandomGame(ITetriNETCallback callback, bool asSpectator)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientJoinRandomGame.Do(x => x(client, asSpectator));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientJoinRandomGame from unknown client");
        }

        public void ClientCreateAndJoinGame(ITetriNETCallback callback, string name, string password, GameRules rule, bool asSpectator)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientCreateAndJoinGame.Do(x => x(client, name, password, rule, asSpectator));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientCreateAndJoinGame from unknown client");
        }

        public void ClientGetRoomList(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientGetRoomList.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientGetRoomList from unknown client");
        }

        public void ClientStartGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientStartGame.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientStartGame from unknown client");
        }

        public void ClientStopGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientStopGame.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientStopGame from unknown client");
        }

        public void ClientPauseGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientPauseGame.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientPauseGame from unknown client");
        }

        public void ClientResumeGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientResumeGame.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientResumeGame from unknown client");
        }

        public void ClientChangeOptions(ITetriNETCallback callback, GameOptions options)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientChangeOptions.Do(x => x(client, options));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientChangeOptions from unknown client");
        }

        public void ClientVoteKick(ITetriNETCallback callback, Guid targetId, string reason)
        {
            IClient client = ClientManager[callback];
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

        public void ClientVoteKickResponse(ITetriNETCallback callback, bool accepted)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientVoteKickAnswer.Do(x => x(client, accepted));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientVoteKickResponse from unknown client");
        }

        public void ClientResetWinList(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientResetWinList.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientResetWinList from unknown client");
        }

        public void ClientLeaveGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientLeaveGame.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientLeaveGame from unknown client");
        }

        public void ClientPlacePiece(ITetriNETCallback callback, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientPlacePiece.Do(x => x(client, pieceIndex, highestIndex, piece, orientation, posX, posY, grid));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientPlacePiece from unknown client");
        }

        public void ClientModifyGrid(ITetriNETCallback callback, byte[] grid)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientModifyGrid.Do(x => x(client, grid));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientModifyGrid from unknown client");
        }

        public void ClientUseSpecial(ITetriNETCallback callback, Guid targetId, Specials special)
        {
            IClient client = ClientManager[callback];
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

        public void ClientClearLines(ITetriNETCallback callback, int count)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientClearLines.Do(x => x(client, count));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientClearLines from unknown client");
        }

        public void ClientGameLost(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientGameLost.Do(x => x(client));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientGameLost from unknown client");
        }

        public void ClientFinishContinuousSpecial(ITetriNETCallback callback, Specials special)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientFinishContinuousSpecial.Do(x => x(client, special));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientFinishContinuousSpecial from unknown client");
        }

        public void ClientEarnAchievement(ITetriNETCallback callback, int achievementId, string achievementTitle)
        {
            IClient client = ClientManager[callback];
            if (client != null)
                HostClientEarnAchievement.Do(x => x(client, achievementId, achievementTitle));
            else
                Log.Default.WriteLine(LogLevels.Warning, "ClientEarnAchievement from unknown client");
        }

        #endregion

        #endregion
    }
}
