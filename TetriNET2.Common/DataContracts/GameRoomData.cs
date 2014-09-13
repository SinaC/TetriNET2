using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public class GameRoomData
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<ClientData> Clients { get; set; }

        [DataMember]
        public GameRules Rules { get; set; }
    }
}
