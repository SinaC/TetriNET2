using System.ComponentModel;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public enum LeaveReasons
    {
        [EnumMember]
        [Description("Disconnected")]
        Disconnected, // client called Unregister

        [EnumMember]
        [Description("Crashed")]
        ConnectionLost, // exception while calling client callback

        [EnumMember]
        [Description("Timeout")]
        Timeout, // timeout

        [EnumMember]
        [Description("Vote kicked")]
        Kick, // vote kicked

        [EnumMember]
        [Description("Banned")]
        Ban, // banned by server

        [EnumMember]
        [Description("Kicked because of spam")]
        Spam // kicked by host because of spam
    }
}
