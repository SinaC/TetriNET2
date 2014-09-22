namespace TetriNET2.Server.Interfaces.IHost
{
    public partial interface IHost
    {
        //
        IBanManager BanManager { get; }
        IClientManager ClientManager { get; }
        IAdminManager AdminManager { get; }
        IGameRoomManager GameRoomManager { get; }

        //
        void Start();
        void Stop();

        // Called when a client/admin/room is removed from client/admin/room manager
        void AddClient(IClient added);
        void AddAdmin(IAdmin added);
        void AddGameRoom(IGameRoom added);
        void RemoveClient(IClient removed);
        void RemoveAdmin(IAdmin removed);
        void RemoveGameRoom(IGameRoom removed);
    }
}
