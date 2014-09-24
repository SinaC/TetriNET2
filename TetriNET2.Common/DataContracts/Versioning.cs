using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public sealed class Versioning
    {
        [DataMember]
        public int Major { get; set; }

        [DataMember]
        public int Minor { get; set; }
    }
}
