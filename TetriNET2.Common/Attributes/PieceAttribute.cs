using System;

namespace TetriNET2.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class PieceAttribute : Attribute
    {
        public bool Available { get; }
        public string Name { get; }

        public PieceAttribute(bool available)
        {
            Available = available;
        }

        public PieceAttribute(bool available, string name)
            : this(available)
        {
            Name = name;
        }
    }
}
