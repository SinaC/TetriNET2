using System.ComponentModel;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public enum GameCreateResults
    {
        [EnumMember]
        [Description("Successfull")]
        Successfull,

        [EnumMember]
        [Description("Already exists")]
        FailedAlreadyExists,

        [EnumMember]
        [Description("Too many rooms")]
        FailedTooManyRooms,

        [EnumMember]
        [Description("Internal error")]
        FailedInternalError,

        [EnumMember]
        [Description("Already in a game")]
        FailedAlreadyInGame,
    }
}
