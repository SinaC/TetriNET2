using System.ComponentModel;
using System.Runtime.Serialization;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public enum GameRoomStates
    {
        [EnumMember]
        [Description("Created")]
        Created, // -> WaitStartGame

        [EnumMember]
        [Description("Wait start")]
        WaitStartGame,  // -> StartingGame

        [EnumMember]
        [Description("Starting")]
        StartingGame,   // -> GameStarted

        [EnumMember]
        [Description("Started")]
        GameStarted,    // -> GameFinished | GamePaused

        [EnumMember]
        [Description("Finished")]
        GameFinished,   // -> WaitStartGame

        [EnumMember]
        [Description("Paused")]
        GamePaused, // -> GameStarted

        [EnumMember]
        [Description("Stopping")]
        Stopping,
    }
}
