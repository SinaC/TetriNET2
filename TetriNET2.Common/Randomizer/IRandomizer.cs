namespace TetriNET2.Common.Randomizer
{
    public interface IRandomizer
    {
        int Next();
        int Next(int maxValue);
        int Next(int minValue, int maxValue);
    }
}
