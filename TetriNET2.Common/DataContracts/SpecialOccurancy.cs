using System.Runtime.Serialization;
using TetriNET2.Common.Occurancy;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public class SpecialOccurancy : IOccurancy<Specials>
    {
        [DataMember]
        public Specials Value { get; set; }

        [DataMember]
        public int Occurancy { get; set; }
    }
}
