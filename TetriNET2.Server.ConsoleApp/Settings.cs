using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server.ConsoleApp
{
    public class Settings : ISettings
    {
        public int MaxAdmins => 5;
        public int MaxClients => 50;
        public int MaxGames => 10;
        public string BanFilename => @"D:\TEMP\ban.lst";
    }
}
