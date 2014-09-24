using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public sealed class GameStatistics
    {
        [DataMember]
        public DateTime GameStarted { get; set; }

        [DataMember]
        public DateTime GameFinished { get; set; }

        [DataMember]
        public List<GameStatisticsByPlayer> Players { get; set; }
    }
}
