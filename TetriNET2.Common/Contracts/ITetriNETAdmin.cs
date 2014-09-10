using System;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Common.Contracts
{
    public interface ITetriNETAdmin
    {
        // Connect/disconnect
        void AdminConnect(ITetriNETAdminCallback callback, Versioning version, string name, string password);
        void AdminDisconnect(ITetriNETAdminCallback callback);

        // Messaging
        void AdminSendPrivateAdminMessage(ITetriNETAdminCallback callback, Guid targetAdminId, string message);
        void AdminSendPrivateMessage(ITetriNETAdminCallback callback, Guid targetClientId, string message);
        void AdminSendBroadcastMessage(ITetriNETAdminCallback callback, string message);

        // Monitoring
        void AdminGetAdminList(ITetriNETAdminCallback callback);
        void AdminGetClientList(ITetriNETAdminCallback callback);
        void AdminGetClientListInRoom(ITetriNETAdminCallback callback, Guid roomId);
        void AdminGetRoomList(ITetriNETAdminCallback callback);
        void AdminGetBannedList(ITetriNETAdminCallback callback);

        // Kick/Ban
        void AdminKick(ITetriNETAdminCallback callback, Guid targetId, string reason);
        void AdminBan(ITetriNETAdminCallback callback, Guid targetId, string reason);

        // Server commands
        void AdminRestartServer(ITetriNETAdminCallback callback, int seconds);
    }
}
