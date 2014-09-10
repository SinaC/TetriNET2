using System;

namespace TetriNET2.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class GameRuleAttribute : Attribute
    {
        public bool AreOccurancesReadonly { get; private set; }

        public GameRuleAttribute(bool isReadonly)
        {
            AreOccurancesReadonly = isReadonly;
        }
    }
}
