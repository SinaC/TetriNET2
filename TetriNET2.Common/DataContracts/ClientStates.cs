using System.ComponentModel;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public enum ClientStates
    {
        [EnumMember]
        [Description("Connected")]
        Connected, // -> WaitInGameRoom

        [EnumMember]
        [Description("Wait in game room")]
        WaitInGameRoom, // -> Connected | Playing

        [EnumMember]
        [Description("Playing")]
        Playing,    // -> Connected | GameLost

        [EnumMember]
        [Description("Game lost")]
        GameLost,   // -> Connected | WaitInGameRoom
    }
}
