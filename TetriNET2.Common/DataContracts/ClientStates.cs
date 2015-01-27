using System.ComponentModel;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public enum ClientStates
    {
        [EnumMember]
        [Description("Connected")]
        Connected, // -> WaitInGame

        [EnumMember]
        [Description("Wait in game")]
        WaitInGame, // -> Connected | Playing

        [EnumMember]
        [Description("Playing")]
        Playing,    // -> Connected | GameLost

        [EnumMember]
        [Description("Game lost")]
        GameLost,   // -> Connected | WaitInGame
    }
}
