using System;
using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;

namespace TetriNET2.Server.WCFHost
{
    public partial class WCFHost : IHost
    {
        public partial class WCFServiceHost
        {
        }

        #region ITetriNETAdminHost

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
        
        #region ITetriNETAdmin

        public void AdminConnect(ITetriNETAdminCallback callback, Versioning version, string name, string password)
        {
            HostAdminConnect.Do(x => x(callback, IPAddress.Any, version, name, password)); // TODO: address
        }

        public void AdminDisconnect(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null)
                HostAdminDisconnect.Do(x => x(admin));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminDisconnect from unknown admin");
        }

        public void AdminSendPrivateAdminMessage(ITetriNETAdminCallback callback, Guid targetAdminId, string message)
        {
            IAdmin admin = AdminManager[callback];
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

        public void AdminSendPrivateMessage(ITetriNETAdminCallback callback, Guid targetClientId, string message)
        {
            IAdmin admin = AdminManager[callback];
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

        public void AdminSendBroadcastMessage(ITetriNETAdminCallback callback, string message)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null)
                HostAdminSendBroadcastMessage.Do(x => x(admin, message));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminSendBroadcastMessage from unknown admin");
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

        #endregion
    }
}
