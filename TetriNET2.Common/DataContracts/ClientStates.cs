using System.ComponentModel;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public enum ClientStates
    {
        [EnumMember]
        [Description("In wait room")]
        InWaitRoom, // -> InGameRoom

        [EnumMember]
        [Description("In game room")]
        InGameRoom, // -> InWaitRoom | Playing

        [EnumMember]
        [Description("Playing")]
        Playing,    // -> InWaitRoom | GameLost

        [EnumMember]
        [Description("Game lost")]
        GameLost,   // -> InWaitRoom | InGameRoom
    }
}
