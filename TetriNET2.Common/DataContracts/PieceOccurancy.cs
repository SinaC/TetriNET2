using System.Runtime.Serialization;
using TetriNET2.Common.Occurancy;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public class PieceOccurancy : IOccurancy<Pieces>
    {
        [DataMember]
        public Pieces Value { get; set; }

        [DataMember]
        public int Occurancy { get; set; }
    }
}
