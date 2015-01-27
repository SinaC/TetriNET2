using System;
using System.Net;
using System.Runtime.CompilerServices;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;

namespace TetriNET2.Server.Tests.Mocking
{
    public sealed class HostMock : IHost
    {
        public HostMock(IBanManager banManager, IClientManager clientManager, IAdminManager adminManager, IGameManager gameManager)
        {
            BanManager = banManager;
            ClientManager = clientManager;
            AdminManager = adminManager;
            GameManager = gameManager;
        }

        public IPAddress Address { private get; set; }

        public ITetriNETClientCallback ClientCallback { private get; set; }

        public ITetriNETAdminCallback AdminCallback { private get; set; }

        #region IHost

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
        public event HostAdminConnectEventHandler HostAdminConnect;
        public event HostAdminDisconnectEventHandler HostAdminDisconnect;
        public event HostAdminSendPrivateAdminMessageEventHandler HostAdminSendPrivateAdminMessage;
        public event HostAdminSendPrivateMessageEventHandler HostAdminSendPrivateMessage;
        public event HostAdminSendBroadcastMessageEventHandler HostAdminSendBroadcastMessage;
        public event HostAdminGetAdminListEventHandler HostAdminGetAdminList;
        public event HostAdminGetClientListEventHandler HostAdminGetClientList;
        public event HostAdminGetClientListInGameEventHandler HostAdminGetClientListInGame;
        public event HostAdminGetGameListEventHandler HostAdminGetGameList;
        public event HostAdminGetBannedListEventHandler HostAdminGetBannedList;
        public event HostAdminCreateGameEventHandler HostAdminCreateGame;
        public event HostAdminDeleteGameEventHandler HostAdminDeleteGame;
        public event HostAdminKickEventHandler HostAdminKick;
        public event HostAdminBanEventHandler HostAdminBan;
        public event HostAdminRestartServerEventHandler HostAdminRestartServer;

        public IBanManager BanManager { get; private set; }
        public IClientManager ClientManager { get; private set; }
        public IGameManager GameManager { get; private set; }
        public IAdminManager AdminManager { get; private set; }

        public void Start()
        {
            // NOP
        }

        public void Stop()
        {
            // NOP
        }

        public void AddClient(IClient added)
        {
            // NOP
        }

        public void AddAdmin(IAdmin added)
        {
            // NOP
        }

        public void AddGame(IGame added)
        {
            // NOP
        }

        public void RemoveClient(IClient removed)
        {
            // NOP
        }

        public void RemoveAdmin(IAdmin removed)
        {
            // NOP
        }

        public void RemoveGame(IGame removed)
        {
            // NOP
        }

        #endregion

        #region ITetriNET

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public void ClientConnect(Versioning version, string name, string team)
        {
            if (HostClientConnect != null)
                HostClientConnect(ClientCallback, Address, version, name, team);
        }

        public void ClientDisconnect()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientDisconnect != null)
                HostClientDisconnect(client);
        }

        public void ClientHeartbeat()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientHeartbeat != null)
                HostClientHeartbeat(client);
        }

        public void ClientSendPrivateMessage(Guid targetId, string message)
        {
            IClient client = ClientManager[ClientCallback];
            IClient target = ClientManager[targetId];
            if (client != null && target != null && HostClientSendPrivateMessage != null)
                HostClientSendPrivateMessage(client, target, message);
        }

        public void ClientSendBroadcastMessage(string message)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientSendBroadcastMessage != null)
                HostClientSendBroadcastMessage(client, message);
        }

        public void ClientChangeTeam(string team)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientChangeTeam != null)
                HostClientChangeTeam(client, team);
        }

        public void ClientJoinGame(Guid gameId, string password, bool asSpectator)
        {
            IClient client = ClientManager[ClientCallback];
            IGame game = GameManager[gameId];
            if (client != null && game != null && HostClientJoinGame != null)
                HostClientJoinGame(client, game, password, asSpectator);
        }

        public void ClientJoinRandomGame(bool asSpectator)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientJoinRandomGame != null)
                HostClientJoinRandomGame(client, asSpectator);
        }

        public void ClientCreateAndJoinGame(string name, string password, GameRules rule, bool asSpectator)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientCreateAndJoinGame != null)
                HostClientCreateAndJoinGame(client, name, password, rule, asSpectator);
        }

        public void ClientGetGameList()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientGetGameList != null)
                HostClientGetGameList(client);
        }

        public void ClientGetClientList()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientGetClientList != null)
                HostClientGetClientList(client);
        }

        public void ClientStartGame()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientStartGame != null)
                HostClientStartGame(client);
        }

        public void ClientStopGame()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientStopGame != null)
                HostClientStopGame(client);
        }

        public void ClientPauseGame()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientPauseGame != null)
                HostClientPauseGame(client);
        }

        public void ClientResumeGame()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientResumeGame != null)
                HostClientResumeGame(client);
        }

        public void ClientChangeOptions(GameOptions options)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientChangeOptions != null)
                HostClientChangeOptions(client, options);
        }

        public void ClientVoteKick(Guid targetId, string reason)
        {
            IClient client = ClientManager[ClientCallback];
            IClient target = ClientManager[targetId];
            if (client != null && target != null && HostClientVoteKick != null)
                HostClientVoteKick(client, target, reason);
        }

        public void ClientVoteKickResponse(bool accepted)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientVoteKickAnswer != null)
                HostClientVoteKickAnswer(client, accepted);
        }

        public void ClientResetWinList()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientResetWinList != null)
                HostClientResetWinList(client);
        }

        public void ClientLeaveGame()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientLeaveGame != null)
                HostClientLeaveGame(client);
        }

        public void ClientGetGameClientList()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientGetGameClientList != null)
                HostClientGetGameClientList(client);
        }

        public void ClientPlacePiece(int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientPlacePiece != null)
                HostClientPlacePiece(client, pieceIndex, highestIndex, piece, orientation, posX, posY, grid);
        }

        public void ClientModifyGrid(byte[] grid)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientModifyGrid != null)
                HostClientModifyGrid(client, grid);
        }

        public void ClientUseSpecial(Guid targetId, Specials special)
        {
            IClient client = ClientManager[ClientCallback];
            IClient target = ClientManager[targetId];
            if (client != null && HostClientUseSpecial != null)
                HostClientUseSpecial(client, target, special);
        }

        public void ClientClearLines(int count)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientClearLines != null)
                HostClientClearLines(client, count);
        }

        public void ClientGameLost()
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientGameLost != null)
                HostClientGameLost(client);
        }

        public void ClientFinishContinuousSpecial(Specials special)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientFinishContinuousSpecial != null)
                HostClientFinishContinuousSpecial(client, special);
        }

        public void ClientEarnAchievement(int achievementId, string achievementTitle)
        {
            IClient client = ClientManager[ClientCallback];
            if (client != null && HostClientEarnAchievement != null)
                HostClientEarnAchievement(client, achievementId, achievementTitle);
        }

        #endregion

        #region ITetriNETAdmin

        public void AdminConnect(Versioning version, string name, string password)
        {
            if (HostAdminConnect != null)
                HostAdminConnect(AdminCallback, Address, version, name, password);
        }

        public void AdminDisconnect()
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null && HostAdminDisconnect != null)
                HostAdminDisconnect(admin);
        }

        public void AdminSendPrivateAdminMessage(Guid targetAdminId, string message)
        {
            IAdmin admin = AdminManager[AdminCallback];
            IAdmin target = AdminManager[targetAdminId];
            if (admin != null && target != null && HostAdminSendPrivateAdminMessage != null)
                HostAdminSendPrivateAdminMessage(admin, target, message);
        }

        public void AdminSendPrivateMessage(Guid targetClientId, string message)
        {
            IAdmin admin = AdminManager[AdminCallback];
            IClient target = ClientManager[targetClientId];
            if (admin != null && HostAdminSendPrivateMessage != null)
                HostAdminSendPrivateMessage(admin, target, message);
        }

        public void AdminSendBroadcastMessage(string message)
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null && HostAdminSendBroadcastMessage != null)
                HostAdminSendBroadcastMessage(admin, message);
        }

        public void AdminGetAdminList()
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null && HostAdminGetAdminList != null)
                HostAdminGetAdminList(admin);
        }

        public void AdminGetClientList()
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null && HostAdminGetClientList != null)
                HostAdminGetClientList(admin);
        }

        public void AdminGetClientListInGame(Guid gameId)
        {
            IAdmin admin = AdminManager[AdminCallback];
            IGame game = GameManager[gameId];
            if (admin != null && game != null && HostAdminGetClientListInGame != null)
                HostAdminGetClientListInGame(admin, game);
        }

        public void AdminGetGameList()
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null && HostAdminGetGameList != null)
                HostAdminGetGameList(admin);
        }

        public void AdminGetBannedList()
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null && HostAdminGetBannedList != null)
                HostAdminGetBannedList(admin);
        }

        public void AdminCreateGame(string name, GameRules rule, string password)
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null && HostAdminCreateGame != null)
                HostAdminCreateGame(admin, name, rule, password);
        }

        public void AdminDeleteGame(Guid gameId)
        {
            IAdmin admin = AdminManager[AdminCallback];
            IGame game = GameManager[gameId];
            if (admin != null && game != null && HostAdminCreateGame != null)
                HostAdminDeleteGame(admin, game);
        }

        public void AdminKick(Guid targetId, string reason)
        {
            IAdmin admin = AdminManager[AdminCallback];
            IClient target = ClientManager[targetId];
            if (admin != null && target != null && HostAdminKick != null)
                HostAdminKick(admin, target, reason);
        }

        public void AdminBan(Guid targetId, string reason)
        {
            IAdmin admin = AdminManager[AdminCallback];
            IClient target = ClientManager[targetId];
            if (admin != null && target != null && HostAdminBan != null)
                HostAdminBan(admin, target, reason);
        }

        public void AdminRestartServer(int seconds)
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null && HostAdminRestartServer != null)
                HostAdminRestartServer(admin, seconds);
        }

        #endregion
    }
}
