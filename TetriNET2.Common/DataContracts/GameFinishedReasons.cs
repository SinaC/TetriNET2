using System.ComponentModel;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public enum GameFinishedReasons
    {
        [EnumMember]
        [Description("Stopped")]
        Stopped,

        [EnumMember]
        [Description("Not enough players left")]
        NotEnoughPlayers,

        [EnumMember]
        [Description("Won")]
        Won,
    }
}
