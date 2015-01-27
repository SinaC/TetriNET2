using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public sealed class GameData
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<ClientData> Clients { get; set; }

        [DataMember]
        public GameRules Rule { get; set; }

        [DataMember]
        public GameOptions Options { get; set; }
    }
}
