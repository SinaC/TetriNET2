using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public class WinEntry
    {
        [DataMember]
        public string PlayerName { get; set; }

        [DataMember(IsRequired = false)]
        public string Team { get; set; }

        [DataMember]
        public int Score { get; set; }
    }
}
