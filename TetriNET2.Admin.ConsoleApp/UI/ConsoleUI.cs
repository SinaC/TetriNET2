using System;
using System.Collections.Generic;
using TetriNET2.Admin.Interfaces;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Admin.ConsoleApp.UI
{
    public class ConsoleUI
    {
        private readonly IAdmin _admin;

        public ConsoleUI(IAdmin admin)
        {
            _admin = admin;


            _admin.Connected += OnConnected;
            _admin.Disconnected += OnDisconnected;

            _admin.ServerStopped += OnServerStopped;

            _admin.ClientConnected += OnClientConnected;
            _admin.ClientDisconnected += OnClientDisconnected;

            _admin.AdminConnected += OnAdminConnected;
            _admin.AdminDisconnected += OnAdminDisconnected;

            _admin.GameCreated += OnGameCreated;
            _admin.GameDeleted += OnGameDeleted;

            _admin.ServerMessageReceived += OnServerMessageReceived;
            _admin.BroadcastMessageReceived += OnBroadcastMessageReceived;
            _admin.PrivateMessageReceived += OnPrivateMessageReceived;

            _admin.AdminListReceived += OnAdminListReceived;
            _admin.ClientListReceived += OnClientListReceived;
            _admin.ClientListInGameReceived += OnClientListInGameReceived;
            _admin.GameListReceived += OnGameListReceived;
            _admin.BannedListReceived += OnBannedListReceived;
        }

        private void DisplayAdminList()
        {
            Console.WriteLine("Admin list: {0}", _admin.Admins.Count);
            foreach (AdminData admin in _admin.Admins)
                Console.WriteLine("Admin: {0} {1} {2:dd-MM-yyyy HH:mm:ss.fff} {3}", admin.Id, admin.Name, admin.ConnectTime, admin.Address);
        }

        private void DisplayClientList()
        {
            Console.WriteLine("Client list: {0}", _admin.Clients.Count);
            foreach (ClientAdminData client in _admin.Clients)
                Console.WriteLine("Client: {0} {1} {2} {3:HH:mm:ss.fff} {4} {5} {6}", client.Id, client.Name, client.Team, client.ConnectTime, client.Address, client.Roles, client.State);
        }

        private void DisplayGameList()
        {
            Console.WriteLine("Games: {0}", _admin.Games.Count);
            foreach (GameAdminData game in _admin.Games)
            {
                Console.WriteLine("Game: {0} {1} {2}", game.Id, game.Name, game.Rule);
                Console.WriteLine("\tClients: {0}", game.Clients?.Count ?? 0);
                if (game.Clients != null)
                    foreach (ClientAdminData client in game.Clients)
                        Console.WriteLine("Client: {0} {1} {2} {3:HH:mm:ss.fff} {4} {5} {6}", client.Id, client.Name, client.Team, client.ConnectTime, client.Address, client.Roles, client.State);
            }
        }

        private void OnConnected(ConnectResults result, Versioning serverVersion, Guid adminId)
        {
            Console.WriteLine("OnConnected: {0} {1}.{2} {3}", result, serverVersion?.Major ?? -1, serverVersion?.Minor ?? -1, adminId);
        }

        private void OnDisconnected()
        {
            Console.WriteLine("OnDisconnected");
        }

        private void OnServerStopped()
        {
            Console.WriteLine("OnServerStopped");
        }

        private void OnClientConnected(Guid clientId, string name, string team)
        {
            Console.WriteLine("OnClientConnected: {0} {1} {2}", clientId, name, team);

            DisplayClientList();
        }

        private void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            Console.WriteLine("OnClientDisconnected: {0} {1}", clientId, reason);

            DisplayClientList();
        }

        private void OnAdminConnected(Guid adminId, string name)
        {
            Console.WriteLine("OnAdminConnected: {0} {1}", adminId, name);

            DisplayAdminList();
        }

        private void OnAdminDisconnected(Guid adminId, LeaveReasons reason)
        {
            Console.WriteLine("OnAdminDisconnected: {0} {1}", adminId, reason);

            DisplayAdminList();
        }

        private void OnGameCreated(bool createdByClient, Guid clientOrAdminId, GameAdminData game)
        {
            Console.WriteLine("OnGameCreated: {0} {1} {2} {3} {4}", createdByClient, clientOrAdminId, game?.Id ?? Guid.Empty, game?.Name ?? string.Empty, game?.Rule ?? GameRules.Custom);
            if (game?.Clients != null)
            {
                Console.WriteLine("\tClients: {0}", game.Clients.Count);
                foreach (ClientAdminData client in game.Clients)
                    Console.WriteLine("\tClient: {0} {1} {2} {3:HH:mm:ss.fff} {4} {5} {6}", client.Id, client.Name, client.Team, client.ConnectTime, client.Address, client.Roles, client.State);
            }

            DisplayGameList();
        }

        private void OnGameDeleted(Guid adminId, Guid gameId)
        {
            Console.WriteLine("OnGameDeleted: {0} {1}", adminId, gameId);

            DisplayGameList();
        }

        private void OnServerMessageReceived(string message)
        {
            Console.WriteLine("OnServerMessageReceived: {0}", message);
        }

        private void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            Console.WriteLine("OnBroadcastMessageReceived: {0} {1}", clientId, message);
        }

        private void OnPrivateMessageReceived(Guid adminId, string message)
        {
            Console.WriteLine("OnPrivateMessageReceived: {0} {1}", adminId, message);
        }

        private void OnAdminListReceived(List<AdminData> admins)
        {
            Console.WriteLine("OnAdminListReceived: {0}", admins?.Count ?? 0);
            if (admins != null)
                foreach (AdminData admin in admins)
                    Console.WriteLine("Admin: {0} {1} {2:HH:mm:ss.fff} {3}", admin.Id, admin.Name, admin.ConnectTime, admin.Address);

            DisplayAdminList();
        }

        private void OnClientListReceived(List<ClientAdminData> clients)
        {
            Console.WriteLine("OnClientListReceived: {0}", clients?.Count ?? 0);
            if (clients != null)
                foreach (ClientAdminData client in clients)
                    Console.WriteLine("Client: {0} {1} {2} {3:HH:mm:ss.fff} {4} {5} {6}", client.Id, client.Name, client.Team, client.ConnectTime, client.Address, client.Roles, client.State);
        }

        private void OnClientListInGameReceived(Guid gameId, List<ClientAdminData> clients)
        {
            Console.WriteLine("OnClientListInGameReceived {0}: {1}", gameId, clients?.Count ?? 0);
            if (clients != null)
                foreach (ClientAdminData client in clients)
                    Console.WriteLine("Client: {0} {1} {2} {3:HH:mm:ss.fff} {4} {5} {6}", client.Id, client.Name, client.Team, client.ConnectTime, client.Address, client.Roles, client.State);
        }

        private void OnGameListReceived(List<GameAdminData> games)
        {
            Console.WriteLine("OnGameListReceived: {0}", games?.Count ?? 0);
            if (games != null)
                foreach (GameAdminData game in games)
                {
                    Console.WriteLine("Game: {0} {1} {2} {3}", game.Id, game.Name, game.Rule, game.State);
                    Console.WriteLine("\tClients: {0}", game.Clients?.Count ?? 0);
                    if (game.Clients != null)
                        foreach (ClientAdminData client in game.Clients)
                            Console.WriteLine("\tClient: {0} {1} {2} {3:HH:mm:ss.fff} {4} {5} {6}", client.Id, client.Name, client.Team, client.ConnectTime, client.Address, client.Roles, client.State);
                }
        }

        private void OnBannedListReceived(List<BanEntryData> entries)
        {
            Console.WriteLine("OnBannedListReceived: {0}", entries?.Count ?? 0);
            if (entries != null)
                foreach (BanEntryData ban in entries)
                    Console.WriteLine("Ban: {0} {1} {2}", ban.Name, ban.Address, ban.Reason);
        }
    }
}
