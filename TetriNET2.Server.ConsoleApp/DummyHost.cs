using System;
using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;

namespace TetriNET2.Server.ConsoleApp
{
    public class DummyHost : IHost
    {
        public DummyHost(IClientManager clientManager, IAdminManager adminManager, IGameRoomManager gameRoomManager)
        {
            ClientManager = clientManager;
            AdminManager = adminManager;
            GameRoomManager = gameRoomManager;
        }

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
        public event HostAdminConnectEventHandler HostAdminConnect;
        public event HostAdminDisconnectEventHandler HostAdminDisconnect;
        public event HostAdminSendPrivateAdminMessageEventHandler HostAdminSendPrivateAdminMessage;
        public event HostAdminSendPrivateMessageEventHandler HostAdminSendPrivateMessage;
        public event HostAdminSendBroadcastMessageEventHandler HostAdminSendBroadcastMessage;
        public event HostAdminGetAdminListEventHandler HostAdminGetAdminList;
        public event HostAdminGetClientListEventHandler HostAdminGetClientList;
        public event HostAdminGetClientListInRoomEventHandler HostAdminGetClientListInRoom;
        public event HostAdminGetRoomListEventHandler HostAdminGetRoomList;
        public event HostAdminGetBannedListEventHandler HostAdminGetBannedList;
        public event HostAdminKickEventHandler HostAdminKick;
        public event HostAdminBanEventHandler HostAdminBan;
        public event HostAdminRestartServerEventHandler HostAdminRestartServer;

        public IClientManager ClientManager { get; private set; }
        public IGameRoomManager GameRoomManager { get; private set; }
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

        public void AddGameRoom(IGameRoom added)
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

        public void RemoveGameRoom(IGameRoom removed)
        {
            // NOP
        }

        #endregion

        #region ITetriNET

        public void ClientConnect(ITetriNETCallback callback, Versioning version, string name, string team)
        {
            if (HostClientConnect != null)
                HostClientConnect(callback, IPAddress.Any, version, name, team);
        }

        public void ClientDisconnect(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientDisconnect != null)
                HostClientDisconnect(client);
        }

        public void ClientHeartbeat(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientHeartbeat != null)
                HostClientHeartbeat(client);
        }

        public void ClientSendPrivateMessage(ITetriNETCallback callback, Guid targetId, string message)
        {
            IClient client = ClientManager[callback];
            IClient target = ClientManager[targetId];
            if (client != null && target != null && HostClientSendPrivateMessage != null)
                HostClientSendPrivateMessage(client, target, message);
        }

        public void ClientSendBroadcastMessage(ITetriNETCallback callback, string message)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientSendBroadcastMessage != null)
                HostClientSendBroadcastMessage(client, message);
        }

        public void ClientChangeTeam(ITetriNETCallback callback, string team)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientChangeTeam != null)
                HostClientChangeTeam(client, team);
        }

        public void ClientJoinGame(ITetriNETCallback callback, Guid gameId, string password, bool asSpectator)
        {
            IClient client = ClientManager[callback];
            IGameRoom game = GameRoomManager[gameId];
            if (client != null && game != null && HostClientJoinGame != null)
                HostClientJoinGame(client, game, password, asSpectator);
        }

        public void ClientJoinRandomGame(ITetriNETCallback callback, bool asSpectator)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientJoinRandomGame != null)
                HostClientJoinRandomGame(client, asSpectator);
        }

        public void ClientCreateAndJoinGame(ITetriNETCallback callback, string name, string password, GameRules rule, bool asSpectator)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientCreateAndJoinGame != null)
                HostClientCreateAndJoinGame(client, name, password, rule, asSpectator);
        }

        public void ClientStartGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientStartGame != null)
                HostClientStartGame(client);
        }

        public void ClientStopGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientStopGame != null)
                HostClientStopGame(client);
        }

        public void ClientPauseGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientPauseGame != null)
                HostClientPauseGame(client);
        }

        public void ClientResumeGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientResumeGame != null)
                HostClientResumeGame(client);
        }

        public void ClientChangeOptions(ITetriNETCallback callback, GameOptions options)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientChangeOptions != null)
                HostClientChangeOptions(client, options);
        }

        public void ClientVoteKick(ITetriNETCallback callback, Guid targetId, string reason)
        {
            IClient client = ClientManager[callback];
            IClient target = ClientManager[targetId];
            if (client != null && target != null && HostClientVoteKick != null)
                HostClientVoteKick(client, target, reason);
        }

        public void ClientVoteKickResponse(ITetriNETCallback callback, bool accepted)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientVoteKickAnswer != null)
                HostClientVoteKickAnswer(client, accepted);
        }

        public void ClientResetWinList(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientResetWinList != null)
                HostClientResetWinList(client);
        }

        public void ClientLeaveGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientLeaveGame != null)
                HostClientLeaveGame(client);
        }

        public void ClientPlacePiece(ITetriNETCallback callback, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientPlacePiece != null)
                HostClientPlacePiece(client, pieceIndex, highestIndex, piece, orientation, posX, posY, grid);
        }

        public void ClientModifyGrid(ITetriNETCallback callback, byte[] grid)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientModifyGrid != null)
                HostClientModifyGrid(client, grid);
        }

        public void ClientUseSpecial(ITetriNETCallback callback, Guid targetId, Specials special)
        {
            IClient client = ClientManager[callback];
            IClient target = ClientManager[targetId];
            if (client != null && HostClientUseSpecial != null)
                HostClientUseSpecial(client, target, special);
        }

        public void ClientClearLines(ITetriNETCallback callback, int count)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientClearLines != null)
                HostClientClearLines(client, count);
        }

        public void ClientGameLost(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientGameLost != null)
                HostClientGameLost(client);
        }

        public void ClientFinishContinuousSpecial(ITetriNETCallback callback, Specials special)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientFinishContinuousSpecial != null)
                HostClientFinishContinuousSpecial(client, special);
        }

        public void ClientEarnAchievement(ITetriNETCallback callback, int achievementId, string achievementTitle)
        {
            IClient client = ClientManager[callback];
            if (client != null && HostClientEarnAchievement != null)
                HostClientEarnAchievement(client, achievementId, achievementTitle);
        }

        #endregion

        #region ITetriNETAdmin

        public void AdminConnect(ITetriNETAdminCallback callback, Versioning version, string name, string password)
        {
            if (HostAdminConnect != null)
                HostAdminConnect(callback, null, version, name, password);
        }

        public void AdminDisconnect(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && HostAdminDisconnect != null)
                HostAdminDisconnect(admin);
        }

        public void AdminSendPrivateAdminMessage(ITetriNETAdminCallback callback, Guid targetAdminId, string message)
        {
            IAdmin admin = AdminManager[callback];
            IAdmin target = AdminManager[targetAdminId];
            if (admin != null && target != null && HostAdminSendPrivateAdminMessage != null)
                HostAdminSendPrivateAdminMessage(admin, target, message);
        }

        public void AdminSendPrivateMessage(ITetriNETAdminCallback callback, Guid targetClientId, string message)
        {
            IAdmin admin = AdminManager[callback];
            IClient target = ClientManager[targetClientId];
            if (admin != null && HostAdminSendPrivateMessage != null)
                HostAdminSendPrivateMessage(admin, target, message);
        }

        public void AdminSendBroadcastMessage(ITetriNETAdminCallback callback, string message)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && HostAdminSendBroadcastMessage != null)
                HostAdminSendBroadcastMessage(admin, message);
        }

        public void AdminGetAdminList(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && HostAdminGetAdminList != null)
                HostAdminGetAdminList(admin);
        }

        public void AdminGetClientList(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && HostAdminGetClientList != null)
                HostAdminGetClientList(admin);
        }

        public void AdminGetClientListInRoom(ITetriNETAdminCallback callback, Guid roomId)
        {
            IAdmin admin = AdminManager[callback];
            IGameRoom game = GameRoomManager[roomId];
            if (admin != null && game != null && HostAdminGetClientListInRoom != null)
                HostAdminGetClientListInRoom(admin, game);
        }

        public void AdminGetRoomList(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && HostAdminGetRoomList != null)
                HostAdminGetRoomList(admin);
        }

        public void AdminGetBannedList(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && HostAdminGetBannedList != null)
                HostAdminGetBannedList(admin);
        }

        public void AdminKick(ITetriNETAdminCallback callback, Guid targetId, string reason)
        {
            IAdmin admin = AdminManager[callback];
            IClient target = ClientManager[targetId];
            if (admin != null && target != null && HostAdminKick != null)
                HostAdminKick(admin, target, reason);
        }

        public void AdminBan(ITetriNETAdminCallback callback, Guid targetId, string reason)
        {
            IAdmin admin = AdminManager[callback];
            IClient target = ClientManager[targetId];
            if (admin != null && target != null && HostAdminBan != null)
                HostAdminBan(admin, target, reason);
        }

        public void AdminRestartServer(ITetriNETAdminCallback callback, int seconds)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && HostAdminRestartServer != null)
                HostAdminRestartServer(admin, seconds);
        }

        #endregion
    }
}
