﻿using System;
using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces
{
    public delegate void ClientConnectionLostEventHandler(IClient entity);

    public interface IClient : ITetriNETCallback
    {
        event ClientConnectionLostEventHandler ConnectionLost;

        //
        Guid Id { get; }
        string Name { get; }
        IPAddress Address { get; }
        ITetriNETCallback Callback { get; }
        DateTime ConnectTime { get; }

        //
        ClientStates State { get; set; }
        ClientRoles Roles { get; set; }

        //
        string Team { get; set; }
        int PieceIndex { get; set; }
        byte[] Grid { get; set; }
        DateTime LossTime { get; set; }
        IGameRoom Game { get; set; }

        //
        bool IsGameMaster { get; }
        bool IsPlayer { get; }
        bool IsSpectator { get; }

        // Heartbeat management
        DateTime LastActionToClient { get; } // used to check if heartbeat is needed

        // Timeout management
        DateTime LastActionFromClient { get; }
        int TimeoutCount { get; }

        void ResetTimeout();
        void SetTimeout();
    }
}