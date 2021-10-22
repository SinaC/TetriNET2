namespace TetriNET2.Server.Interfaces
{
    public interface ISettings
    {
        int MaxAdmins { get; }
        int MaxClients { get; }
        int MaxGames { get; }
        string BanFilename { get; }
    }
}
