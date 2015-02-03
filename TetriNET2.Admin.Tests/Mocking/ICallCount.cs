namespace TetriNET2.Admin.Tests.Mocking
{
    public interface ICallCount
    {
        int GetCallCount(string callbackName);
        void Reset();
    }
}
