using System;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Tests.Server.Mocking
{
    public class HostMock : IHost
    {
        public HostMock(IClientManager clientManager, IGameRoomManager gameRoomManager, IAdminManager adminManager)
        {
            ClientManager = clientManager;
            GameRoomManager = gameRoomManager;
            AdminManager = adminManager;
        }

        #region IHost

        public event ClientConnectEventHandler ClientConnect;
        public event ClientDisconnectEventHandler ClientDisconnect;
        public event ClientHeartbeatEventHandler ClientHeartbeat;
        public event ClientSendPrivateMessageEventHandler ClientSendPrivateMessage;
        public event ClientSendBroadcastMessageEventHandler ClientSendBroadcastMessage;
        public event ClientChangeTeamEventHandler ClientChangeTeam;
        public event ClientJoinGameEventHandler ClientJoinGame;
        public event ClientJoinRandomGameEventHandler ClientJoinRandomGame;
        public event ClientCreateAndJoinGameEventHandler ClientCreateAndJoinGame;
        public event ClientStartGameEventHandler ClientStartGame;
        public event ClientStopGameEventHandler ClientStopGame;
        public event ClientPauseGameEventHandler ClientPauseGame;
        public event ClientResumeGameEventHandler ClientResumeGame;
        public event ClientChangeOptionsEventHandler ClientChangeOptions;
        public event ClientVoteKickEventHandler ClientVoteKick;
        public event ClientVoteKickResponseEventHandler ClientVoteKickAnswer;
        public event ClientResetWinListEventHandler ClientResetWinList;
        public event ClientLeaveGameEventHandler ClientLeaveGame;
        public event ClientPlacePieceEventHandler ClientPlacePiece;
        public event ClientModifyGridEventHandler ClientModifyGrid;
        public event ClientUseSpecialEventHandler ClientUseSpecial;
        public event ClientClearLinesEventHandler ClientClearLines;
        public event ClientGameLostEventHandler ClientGameLost;
        public event ClientFinishContinuousSpecialEventHandler ClientFinishContinuousSpecial;
        public event ClientEarnAchievementEventHandler ClientEarnAchievement;
        public event AdminConnectEventHandler AdminConnect;
        public event AdminDisconnectEventHandler AdminDisconnect;
        public event AdminSendPrivateAdminMessageEventHandler AdminSendPrivateAdminMessage;
        public event AdminSendPrivateMessageEventHandler AdminSendPrivateMessage;
        public event AdminSendBroadcastMessageEventHandler AdminSendBroadcastMessage;
        public event AdminGetAdminListEventHandler AdminGetAdminList;
        public event AdminGetClientListEventHandler AdminGetClientList;
        public event AdminGetClientListInRoomEventHandler AdminGetClientListInRoom;
        public event AdminGetRoomListEventHandler AdminGetRoomList;
        public event AdminGetBannedListEventHandler AdminGetBannedList;
        public event AdminKickEventHandler AdminKick;
        public event AdminBanEventHandler AdminBan;
        public event AdminRestartServerEventHandler AdminRestartServer;

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

        public void AddRoom(IGameRoom added)
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

        void ITetriNET.ClientConnect(ITetriNETCallback callback, Versioning version, string name, string team)
        {
            if (ClientConnect != null)
                ClientConnect(callback, null, version, name, team);
        }

        void ITetriNET.ClientDisconnect(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientDisconnect != null)
                ClientDisconnect(client);
        }

        void ITetriNET.ClientHeartbeat(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientHeartbeat != null)
                ClientHeartbeat(client);
        }

        void ITetriNET.ClientSendPrivateMessage(ITetriNETCallback callback, Guid targetId, string message)
        {
            IClient client = ClientManager[callback];
            IClient target = ClientManager[targetId];
            if (client != null && target != null && ClientSendPrivateMessage != null)
                ClientSendPrivateMessage(client, target, message);
        }

        void ITetriNET.ClientSendBroadcastMessage(ITetriNETCallback callback, string message)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientSendBroadcastMessage != null)
                ClientSendBroadcastMessage(client, message);
        }

        void ITetriNET.ClientChangeTeam(ITetriNETCallback callback, string team)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientChangeTeam != null)
                ClientChangeTeam(client, team);
        }

        void ITetriNET.ClientJoinGame(ITetriNETCallback callback, Guid gameId, string password, bool asSpectator)
        {
            IClient client = ClientManager[callback];
            IGameRoom game = GameRoomManager[gameId];
            if (client != null && game != null && ClientJoinGame != null)
                ClientJoinGame(client, game, password, asSpectator);
        }

        void ITetriNET.ClientJoinRandomGame(ITetriNETCallback callback, bool asSpectator)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientJoinRandomGame != null)
                ClientJoinRandomGame(client, asSpectator);
        }

        void ITetriNET.ClientCreateAndJoinGame(ITetriNETCallback callback, string name, string password, GameRules rule, bool asSpectator)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientCreateAndJoinGame != null)
                ClientCreateAndJoinGame(client, name, password, rule, asSpectator);
        }

        void ITetriNET.ClientStartGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientStartGame != null)
                ClientStartGame(client);
        }

        void ITetriNET.ClientStopGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientStopGame != null)
                ClientStopGame(client);
        }

        void ITetriNET.ClientPauseGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientPauseGame != null)
                ClientPauseGame(client);
        }

        void ITetriNET.ClientResumeGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientResumeGame != null)
                ClientResumeGame(client);
        }

        void ITetriNET.ClientChangeOptions(ITetriNETCallback callback, GameOptions options)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientChangeOptions != null)
                ClientChangeOptions(client, options);
        }

        void ITetriNET.ClientVoteKick(ITetriNETCallback callback, Guid targetId)
        {
            IClient client = ClientManager[callback];
            IClient target = ClientManager[targetId];
            if (client != null && target != null && ClientVoteKick != null)
                ClientVoteKick(client, target);
        }

        public void ClientVoteKickResponse(ITetriNETCallback callback, bool accepted)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientVoteKickAnswer != null)
                ClientVoteKickAnswer(client, accepted);
        }

        void ITetriNET.ClientResetWinList(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientResetWinList != null)
                ClientResetWinList(client);
        }

        void ITetriNET.ClientLeaveGame(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientLeaveGame != null)
                ClientLeaveGame(client);
        }

        void ITetriNET.ClientPlacePiece(ITetriNETCallback callback, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientPlacePiece != null)
                ClientPlacePiece(client, pieceIndex, highestIndex, piece, orientation, posX, posY, grid);
        }

        void ITetriNET.ClientModifyGrid(ITetriNETCallback callback, byte[] grid)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientModifyGrid != null)
                ClientModifyGrid(client, grid);
        }

        void ITetriNET.ClientUseSpecial(ITetriNETCallback callback, Guid targetId, Specials special)
        {
            IClient client = ClientManager[callback];
            IClient target = ClientManager[targetId];
            if (client != null && ClientUseSpecial != null)
                ClientUseSpecial(client, target, special);
        }

        void ITetriNET.ClientClearLines(ITetriNETCallback callback, int count)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientClearLines != null)
                ClientClearLines(client, count);
        }

        void ITetriNET.ClientGameLost(ITetriNETCallback callback)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientGameLost != null)
                ClientGameLost(client);
        }

        void ITetriNET.ClientFinishContinuousSpecial(ITetriNETCallback callback, Specials special)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientFinishContinuousSpecial != null)
                ClientFinishContinuousSpecial(client, special);
        }

        void ITetriNET.ClientEarnAchievement(ITetriNETCallback callback, int achievementId, string achievementTitle)
        {
            IClient client = ClientManager[callback];
            if (client != null && ClientEarnAchievement != null)
                ClientEarnAchievement(client, achievementId, achievementTitle);
        }
 
        #endregion

        #region ITetriNETAdmin

        void ITetriNETAdmin.AdminConnect(ITetriNETAdminCallback callback, Versioning version, string name, string password)
        {
            if (AdminConnect != null)
                AdminConnect(callback, null, version, name, password);
        }

        void ITetriNETAdmin.AdminDisconnect(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && AdminDisconnect != null)
                AdminDisconnect(admin);
        }

        void ITetriNETAdmin.AdminSendPrivateAdminMessage(ITetriNETAdminCallback callback, Guid targetAdminId, string message)
        {
            IAdmin admin = AdminManager[callback];
            IAdmin target = AdminManager[targetAdminId];
            if (admin != null && target != null && AdminSendPrivateAdminMessage != null)
                AdminSendPrivateAdminMessage(admin, target, message);
        }

        void ITetriNETAdmin.AdminSendPrivateMessage(ITetriNETAdminCallback callback, Guid targetClientId, string message)
        {
            IAdmin admin = AdminManager[callback];
            IClient target = ClientManager[targetClientId];
            if (admin != null && AdminSendPrivateMessage != null)
                AdminSendPrivateMessage(admin, target, message);
        }

        void ITetriNETAdmin.AdminSendBroadcastMessage(ITetriNETAdminCallback callback, string message)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && AdminSendBroadcastMessage != null)
                AdminSendBroadcastMessage(admin, message);
        }

        void ITetriNETAdmin.AdminGetAdminList(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && AdminGetAdminList != null)
                AdminGetAdminList(admin);
        }

        void ITetriNETAdmin.AdminGetClientList(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && AdminGetClientList != null)
                AdminGetClientList(admin);
        }

        void ITetriNETAdmin.AdminGetClientListInRoom(ITetriNETAdminCallback callback, Guid roomId)
        {
            IAdmin admin = AdminManager[callback];
            IGameRoom game = GameRoomManager[roomId];
            if (admin != null && game != null && AdminGetClientListInRoom != null)
                AdminGetClientListInRoom(admin, game);
        }

        void ITetriNETAdmin.AdminGetRoomList(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && AdminGetRoomList != null)
                AdminGetRoomList(admin);
        }

        void ITetriNETAdmin.AdminGetBannedList(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && AdminGetBannedList != null)
                AdminGetBannedList(admin);
        }

        void ITetriNETAdmin.AdminKick(ITetriNETAdminCallback callback, Guid targetId, string reason)
        {
            IAdmin admin = AdminManager[callback];
            IClient target = ClientManager[targetId];
            if (admin != null && target != null && AdminKick != null)
                AdminKick(admin, target, reason);
        }

        void ITetriNETAdmin.AdminBan(ITetriNETAdminCallback callback, Guid targetId, string reason)
        {
            IAdmin admin = AdminManager[callback];
            IClient target = ClientManager[targetId];
            if (admin != null && target != null && AdminBan != null)
                AdminBan(admin, target, reason);
        }

        void ITetriNETAdmin.AdminRestartServer(ITetriNETAdminCallback callback, int seconds)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && AdminRestartServer != null)
                AdminRestartServer(admin, seconds);
        }

        #endregion
    }
}
