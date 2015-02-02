using System;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public sealed class ClientData
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Team { get; set; }

        [DataMember]
        public Guid GameId { get; set; }

        [DataMember]
        public bool IsPlayer { get; set; }

        [DataMember]
        public bool IsSpectator { get; set; }

        [DataMember]
        public bool IsGameMaster { get; set; }
    }
}
