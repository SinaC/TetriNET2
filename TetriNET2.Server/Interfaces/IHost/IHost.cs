namespace TetriNET2.Server.Interfaces.IHost
{
    public partial interface IHost
    {
        //
        IBanManager BanManager { get; }
        IClientManager ClientManager { get; }
        IAdminManager AdminManager { get; }
        IGameManager GameManager { get; }

        //
        void Start();
        void Stop();

        // Called when a client/admin/game is removed from client/admin/game manager
        void AddClient(IClient added);
        void AddAdmin(IAdmin added);
        void AddGame(IGame added);
        void RemoveClient(IClient removed);
        void RemoveAdmin(IAdmin removed);
        void RemoveGame(IGame removed);
    }
}
