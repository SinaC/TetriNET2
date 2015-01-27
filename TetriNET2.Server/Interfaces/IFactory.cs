using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces
{
    public interface IFactory
    {
        IClient CreateClient(string name, string team, IPAddress address, ITetriNETClientCallback callback);
        IAdmin CreateAdmin(string name, IPAddress address, ITetriNETAdminCallback callback);
        IGame CreateGame(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password);
    }
}
