﻿using System;
using System.ServiceModel;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Common.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ITetriNETAdminCallback))]
    public interface ITetriNETAdmin
    {
        // Connect/disconnect
        [OperationContract(IsOneWay = true)]
        void AdminConnect(Versioning version, string name, string password);
        [OperationContract(IsOneWay = true)]
        void AdminDisconnect();

        // Messaging
        [OperationContract(IsOneWay = true)]
        void AdminSendPrivateAdminMessage(Guid targetAdminId, string message);
        [OperationContract(IsOneWay = true)]
        void AdminSendPrivateMessage(Guid targetClientId, string message);
        [OperationContract(IsOneWay = true)]
        void AdminSendBroadcastMessage(string message);

        // Monitoring
        [OperationContract(IsOneWay = true)]
        void AdminGetAdminList();
        [OperationContract(IsOneWay = true)]
        void AdminGetClientList();
        [OperationContract(IsOneWay = true)]
        void AdminGetClientListInGame(Guid gameId);
        [OperationContract(IsOneWay = true)]
        void AdminGetGameList();
        [OperationContract(IsOneWay = true)]
        void AdminGetBannedList();

        // Game
        [OperationContract(IsOneWay = true)]
        void AdminCreateGame(string name, GameRules rule, string password);
        [OperationContract(IsOneWay = true)]
        void AdminDeleteGame(Guid gameId);

        // Kick/Ban
        [OperationContract(IsOneWay = true)]
        void AdminKick(Guid targetId, string reason);
        [OperationContract(IsOneWay = true)]
        void AdminBan(Guid targetId, string reason);

        // Server commands
        [OperationContract(IsOneWay = true)]
        void AdminRestartServer(int seconds);
    }
}
