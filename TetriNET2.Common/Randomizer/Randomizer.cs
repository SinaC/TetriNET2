using System;

namespace TetriNET2.Common.Randomizer
{
    public sealed class Randomizer : IRandomizer
    {
        private readonly Random _random = new Random();

        #region Singleton

        private static readonly Lazy<Randomizer> Lazy = new Lazy<Randomizer>(() => new Randomizer());

        public static Randomizer Instance
        {
            get { return Lazy.Value; }
        }

        private Randomizer()
        {
        }

        #endregion

        public int Next()
        {
            return _random.Next();
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
    }
}
