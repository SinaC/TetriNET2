using System.ComponentModel;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public enum GameJoinResults
    {
        [EnumMember]
        [Description("Successfull")]
        Successfull,

        [EnumMember]
        [Description("Wrong password")]
        FailedWrongPassword,
        
        [EnumMember]
        [Description("Too many players")]
        FailedTooManyPlayers,

        [EnumMember]
        [Description("Too many spectators")]
        FailedTooManySpectators,

        [EnumMember]
        [Description("Internal error")]
        FailedInternalError,

        [EnumMember]
        [Description("No room available")]
        FailedNoRoomAvailable,

        [EnumMember]
        [Description("Already in a game")]
        FailedAlreadyInGame,
    }
}
