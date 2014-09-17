using System.ComponentModel;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public enum ConnectResults
    {
        [EnumMember]
        [Description("Successfull")]
        Successfull,

        [EnumMember]
        [Description("Too many clients")]
        FailedTooManyClients,

        [EnumMember]
        [Description("Too many admins")]
        FailedTooManyAdmins,

        [EnumMember]
        [Description("Already exists")]
        FailedClientAlreadyExists,

        [EnumMember]
        [Description("Already exists")]
        FailedAdminAlreadyExists,

        [EnumMember]
        [Description("Invalid name")]
        FailedInvalidName,

        [EnumMember]
        [Description("Internal error")]
        FailedInternalError,

        [EnumMember]
        [Description("Banned")]
        FailedBanned,

        [EnumMember]
        [Description("Incompatible version")]
        FailedIncompatibleVersion,
    }
}
