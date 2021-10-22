using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces
{
    public interface IFactory
    {
        IClient CreateClient(string name, string team, IAddress address, ITetriNETClientCallback callback);
        IAdmin CreateAdmin(string name, IAddress address, ITetriNETAdminCallback callback);
        IGame CreateGame(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password);
    }
}
