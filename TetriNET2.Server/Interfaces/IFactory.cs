using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces
{
    public interface IFactory
    {
        IClient CreateClient(string name, string team, IPAddress address, ITetriNETCallback callback);
        IGameRoom CreateRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password);
        IPieceProvider CreatePieceProvider();
    }
}
