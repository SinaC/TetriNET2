using System;
using TetriNET2.Client.Interfaces;

namespace TetriNET2.Client.Models
{
    public class ClientInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Team { get; set; }

        public Guid GameId { get; set; }

        public bool IsPlayer { get; set; }
        public bool IsSpectator { get; set; }
        public bool IsGameMaster { get; set; }

        public bool IsImmune { get; set; }
        public IBoard Board { get; set; }
    }
}
