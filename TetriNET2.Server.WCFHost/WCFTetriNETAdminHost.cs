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
        private ITetriNETAdminCallback AdminCallback => OperationContext.Current.GetCallbackChannel<ITetriNETAdminCallback>();

        #region ITetriNETAdminHost

        public event AdminConnectEventHandler HostAdminConnect;
        public event AdminDisconnectEventHandler HostAdminDisconnect;

        public event AdminSendPrivateAdminMessageEventHandler HostAdminSendPrivateAdminMessage;
        public event AdminSendPrivateMessageEventHandler HostAdminSendPrivateMessage;
        public event AdminSendBroadcastMessageEventHandler HostAdminSendBroadcastMessage;

        public event AdminGetAdminListEventHandler HostAdminGetAdminList;
        public event AdminGetClientListEventHandler HostAdminGetClientList;
        public event AdminGetClientListInGameEventHandler HostAdminGetClientListInGame;
        public event AdminGetGameListEventHandler HostAdminGetGameList;
        public event AdminGetBannedListEventHandler HostAdminGetBannedList;

        public event AdminCreateGameEventHandler HostAdminCreateGame;
        public event AdminDeleteGameEventHandler HostAdminDeleteGame;

        public event AdminKickEventHandler HostAdminKick;
        public event AdminBanEventHandler HostAdminBan;

        public event AdminRestartServerEventHandler HostAdminRestartServer;

        #region ITetriNETAdmin

        public void AdminConnect(Versioning version, string name, string password)
        {
            HostAdminConnect.Do(x => x(AdminCallback, Address, version, name, password));
        }

        public void AdminDisconnect()
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
                HostAdminDisconnect.Do(x => x(admin));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminDisconnect from unknown admin");
        }

        public void AdminSendPrivateAdminMessage(Guid targetAdminId, string message)
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
            {
                IAdmin target = AdminManager[targetAdminId];
                if (target != null)
                    HostAdminSendPrivateAdminMessage.Do(x => x(admin, target, message));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "AdminSendPrivateAdminMessage to unknown admin");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminSendPrivateAdminMessage from unknown admin");
        }

        public void AdminSendPrivateMessage(Guid targetClientId, string message)
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
            {
                IClient target = ClientManager[targetClientId];
                if (target != null)
                    HostAdminSendPrivateMessage.Do(x => x(admin, target, message));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "AdminSendPrivateMessage to unknown client");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminSendPrivateMessage from unknown admin");
        }

        public void AdminSendBroadcastMessage(string message)
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
                HostAdminSendBroadcastMessage.Do(x => x(admin, message));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminSendBroadcastMessage from unknown admin");
        }

        public void AdminGetAdminList()
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
                HostAdminGetAdminList.Do(x => x(admin));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminGetAdminList from unknown admin");
        }

        public void AdminGetClientList()
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
                HostAdminGetClientList.Do(x => x(admin));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminGetClientList from unknown admin");
        }

        public void AdminGetClientListInGame(Guid gameId)
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
            {
                IGame game = GameManager[gameId];
                if (game != null)
                    HostAdminGetClientListInGame.Do(x => x(admin, game));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "AdminGetClientListInGame for unknown game");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminGetClientListInGame from unknown admin");
        }

        public void AdminGetGameList()
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
                HostAdminGetGameList.Do(x => x(admin));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminGetGameList from unknown admin");
        }

        public void AdminGetBannedList()
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
                HostAdminGetBannedList.Do(x => x(admin));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminGetBannedList from unknown admin");
        }

        public void AdminCreateGame(string name, GameRules rule, string password)
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
                HostAdminCreateGame.Do(x => x(admin, name, rule, password));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminCreateGame from unknown admin");
        }
        
        public void AdminDeleteGame(Guid gameId)
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
            {
                IGame game = GameManager[gameId];
                if (game != null)
                    HostAdminDeleteGame.Do(x => x(admin, game));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "AdminDeleteGame for unknown game");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminDeleteGame from unknown admin");
        }

        public void AdminKick(Guid targetId, string reason)
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
            {
                IClient target = ClientManager[targetId];
                if (target != null)
                    HostAdminKick.Do(x => x(admin, target, reason));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "AdminKick to unknown client");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminKick from unknown admin");
        }

        public void AdminBan(Guid targetId, string reason)
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null)
            {
                IClient target = ClientManager[targetId];
                if (target != null)
                    HostAdminBan.Do(x => x(admin, target, reason));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "AdminBan to unknown client");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminBan from unknown admin");
        }

        public void AdminRestartServer(int seconds)
        {
            IAdmin admin = AdminManager[AdminCallback];
            if (admin != null && HostAdminRestartServer != null)
                HostAdminRestartServer.Do(x => x(admin, seconds));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminRestartServer from unknown admin");
        }

        #endregion

        #endregion
    }
}
