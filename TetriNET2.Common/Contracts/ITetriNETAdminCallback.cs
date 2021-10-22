using System;
using System.Collections.Generic;
using System.ServiceModel;
using TetriNET2.Common.DataContracts;
// ReSharper disable OperationContractWithoutServiceContract

namespace TetriNET2.Common.Contracts
{
    public interface ITetriNETAdminCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnConnected(ConnectResults result, Versioning serverVersion, Guid adminId);
        [OperationContract(IsOneWay = true)]
        void OnDisconnected();

        [OperationContract(IsOneWay = true)]
        void OnServerStopped();

        [OperationContract(IsOneWay = true)]
        void OnClientConnected(Guid clientId, string name, string team, string address);
        [OperationContract(IsOneWay = true)]
        void OnClientDisconnected(Guid clientId, LeaveReasons reason);

        [OperationContract(IsOneWay = true)]
        void OnAdminConnected(Guid adminId, string name, string address);
        [OperationContract(IsOneWay = true)]
        void OnAdminDisconnected(Guid adminId, LeaveReasons reason);

        [OperationContract(IsOneWay = true)]
        void OnGameCreated(bool createdByClient, Guid clientOrAdminId, GameAdminData game);
        [OperationContract(IsOneWay = true)]
        void OnGameDeleted(Guid adminId, Guid gameId);

        [OperationContract(IsOneWay = true)]
        void OnServerMessageReceived(string message);
        [OperationContract(IsOneWay = true)]
        void OnBroadcastMessageReceived(Guid clientId, string message);
        [OperationContract(IsOneWay = true)]
        void OnPrivateMessageReceived(Guid adminId, string message);

        [OperationContract(IsOneWay = true)]
        void OnAdminListReceived(List<AdminData> admins);
        [OperationContract(IsOneWay = true)]
        void OnClientListReceived(List<ClientAdminData> clients);
        [OperationContract(IsOneWay = true)]
        void OnClientListInGameReceived(Guid gameId, List<ClientAdminData> clients);
        [OperationContract(IsOneWay = true)]
        void OnGameListReceived(List<GameAdminData> games);
        [OperationContract(IsOneWay = true)]
        void OnBannedListReceived(List<BanEntryData> entries);
    }
}
