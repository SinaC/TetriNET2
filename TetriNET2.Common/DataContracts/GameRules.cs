using System.Runtime.Serialization;
using TetriNET2.Common.Attributes;

namespace TetriNET2.Common.DataContracts
{
    [DataContract]
    public enum GameRules
    {
        [EnumMember]
        [GameRule(true)]
        Classic, // No special

        [EnumMember]
        [GameRule(true)]
        Standard, // Standard special: AddLine, ClearLine, NukeField, RandomBlockClear, SwitchFields, ClearSpecialBlocks, BlockGravity, BlockQuake, BlockBomb

        [EnumMember]
        [GameRule(true)]
        Extended, // Standard + ClearColumn, Immunity, Darkness, Mutation

        [EnumMember]
        [GameRule(false)]
        Custom, // Every specials
    }
}
