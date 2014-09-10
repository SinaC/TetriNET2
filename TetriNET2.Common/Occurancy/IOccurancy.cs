namespace TetriNET2.Common.Occurancy
{
    public interface IOccurancy<out T>
    {
        T Value { get; }
        int Occurancy { get; }
    }
}
