using System;

namespace TetriNET2.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SpecialAttribute : Attribute
    {
        public bool Available { get; }
        public char ShortName { get; }
        public string LongName { get; }
        public bool Continuous { get; } // One Shot or Continous

        public SpecialAttribute(bool available)
        {
            Available = available;
        }

        public SpecialAttribute(bool available, char shortName, string longName, bool continuous = false)
            : this(available)
        {
            ShortName = shortName;
            LongName = longName;
            Continuous = continuous;
        }
    }
}
