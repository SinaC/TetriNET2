using System;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public sealed class ClientAdminData
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ClientStates State { get; set; }

        [DataMember]
        public ClientRoles Roles { get; set; }

        [DataMember]
        public DateTime ConnectTime { get; set; }

        [DataMember]
        public DateTime LastActionToClient { get; set; }

        [DataMember]
        public DateTime LastActionFromClient { get; set; }

        [DataMember]
        public int TimeoutCount { get; set; }

        [DataMember]
        public string Address { get; set; }
    }
}
