using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    [Flags]
    public enum ClientRoles
    {
        [EnumMember]
        [Description("No role")]
        NoRole = 0x00,

        [EnumMember]
        [Description("Game Master")]
        GameMaster = 0x01,

        [EnumMember]
        [Description("Player")]
        Player = 0x02,

        [EnumMember]
        [Description("Spectator")]
        Spectator = 0x04,
    }
}
