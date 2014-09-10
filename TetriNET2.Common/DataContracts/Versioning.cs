using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public class Versioning
    {
        [DataMember]
        public int Major { get; set; }

        [DataMember]
        public int Minor { get; set; }
    }
}
