using System;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public sealed class AdminData
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime ConnectTime { get; set; }

        [DataMember]
        public string Address { get; set; }
    }
}
