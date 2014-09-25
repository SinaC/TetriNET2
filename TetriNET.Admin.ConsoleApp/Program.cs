using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Admin.ConsoleApp
{
    class Program
    {
        static void DisplayHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop admin");
            Console.WriteLine("a: Get admin list");
            Console.WriteLine("c: Get client list");
            Console.WriteLine("r: Get room list");
            Console.WriteLine("b: Get banned list");
            Console.WriteLine("s: Restart server");
            // TODO: 
            //  get client in room
            //  kick/ban
        }

        static void Main(string[] args)
        {
            CountCallTetriNETAdminCallback callbackInstance = new CountCallTetriNETAdminCallback();

            DuplexChannelFactory<ITetriNETAdmin> factory;
            ITetriNETAdmin proxy;

            EndpointAddress endpointAddress = new EndpointAddress("net.tcp://localhost:7788/TetriNET2Admin");
            Binding binding = new NetTcpBinding(SecurityMode.None);
            InstanceContext instanceContext = new InstanceContext(callbackInstance);
            factory = new DuplexChannelFactory<ITetriNETAdmin>(instanceContext, binding, endpointAddress);
            proxy = factory.CreateChannel(instanceContext);

            proxy.AdminConnect(
                new Versioning
                {
                    Major = 1,
                    Minor = 0
                },
                "admin1", "123456");

            bool stopped = false;
            while (!stopped)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        default:
                            DisplayHelp();
                            break;
                        case ConsoleKey.X:
                            proxy.AdminDisconnect();
                            stopped = true;
                            break;
                        case ConsoleKey.A:
                            proxy.AdminGetAdminList();
                            break;
                        case ConsoleKey.C:
                            proxy.AdminGetClientList();
                            break;
                        case ConsoleKey.R:
                            proxy.AdminGetRoomList();
                            break;
                        case ConsoleKey.B:
                            proxy.AdminGetBannedList();
                            break;
                        case ConsoleKey.S:
                            proxy.AdminRestartServer(90);
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(250);
            }
        }

        public class CountCallTetriNETAdminCallback : ITetriNETAdminCallback
        {
            private class CallInfo
            {
                public static readonly CallInfo NullObject = new CallInfo
                {
                    Count = 0,
                    ParametersPerCall = null
                };

                public int Count { get; set; }
                public List<List<object>> ParametersPerCall { get; set; }
            }

            private readonly Dictionary<string, CallInfo> _callInfos = new Dictionary<string, CallInfo>();

            private void UpdateCallInfo(string callbackName, params object[] parameters)
            {
                Console.WriteLine("Callback: {0} {1}", callbackName, parameters == null || parameters.Length == 0 ? "(none)" : parameters.Select(x => x == null ? "[null]" : x.ToString()).Aggregate((n, i) => n + "[" + i + "]"));

                List<object> paramList = parameters == null ? new List<object>() : parameters.ToList();
                if (!_callInfos.ContainsKey(callbackName))
                    _callInfos.Add(callbackName, new CallInfo
                    {
                        Count = 1,
                        ParametersPerCall = new List<List<object>>
                            {
                                paramList
                            }
                    });
                else
                {
                    CallInfo callInfo = _callInfos[callbackName];
                    callInfo.Count++;
                    callInfo.ParametersPerCall.Add(paramList);
                }
            }

            public int GetCallCount(string callbackName)
            {
                CallInfo value;
                _callInfos.TryGetValue(callbackName, out value);
                return (value ?? CallInfo.NullObject).Count;
            }

            public List<object> GetCallParameters(string callbackName, int callId)
            {
                CallInfo value;
                if (!_callInfos.TryGetValue(callbackName, out value))
                    return null;
                if (callId >= value.Count)
                    return null;
                return value.ParametersPerCall[callId];
            }

            public void Reset()
            {
                _callInfos.Clear();
            }

            #region ITetriNETAdminCallback

            public void OnConnected(ConnectResults result, Versioning serverVersion, Guid adminId)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, result, serverVersion, adminId);
            }

            public void OnDisconnected()
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

            public void OnServerStopped()
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

            public void OnClientConnected(Guid clientId, string name, string team)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, name, team);
            }

            public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, reason);
            }

            public void OnAdminConnected(Guid adminId, string name)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, adminId, name);
            }

            public void OnAdminDisconnected(Guid adminId, LeaveReasons reason)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, adminId, reason);
            }

            public void OnGameCreated(Guid clientId, GameDescription game)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, game);
                Console.WriteLine("Game: {0} {1} {2}", game.Id, game.Name, game.Rule);
                if (game.Players != null)
                {
                    Console.WriteLine("\tPlayers: {0}", game.Players.Count);
                    foreach (string clientName in game.Players)
                        Console.WriteLine("\tPlayer: {0}", clientName);
                }
            }

            public void OnServerMessageReceived(string message)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, message);
            }

            public void OnBroadcastMessageReceived(Guid clientId, string message)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, message);
            }

            public void OnPrivateMessageReceived(Guid adminId, string message)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, adminId, message);
            }

            public void OnAdminListReceived(List<AdminData> admins)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, admins);
                Console.WriteLine("Admins: {0}", admins == null ? 0 : admins.Count);
                if (admins != null)
                    foreach(AdminData admin in admins)
                        Console.WriteLine("Admin: {0} {1} {2:dd-MM-yyyy HH:mm:ss.fff} {3}", admin.Id, admin.Name, admin.ConnectTime, admin.Address);
            }

            public void OnClientListReceived(List<ClientAdminData> clients)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clients);
                Console.WriteLine("Clients: {0}", clients == null ? 0 : clients.Count);
                if (clients != null)
                    foreach(ClientAdminData client in clients)
                        Console.WriteLine("Client: {0} {1} {2:dd-MM-yyyy HH:mm:ss.fff} {3}", client.Id, client.Name, client.ConnectTime, client.Address);
            }

            public void OnClientListInRoomReceived(Guid roomId, List<ClientAdminData> clients)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, roomId, clients);
                Console.WriteLine("Clients in room {0}: {1}", roomId, clients == null ? 0 : clients.Count);
                if (clients != null)
                    foreach (ClientAdminData client in clients)
                        Console.WriteLine("Client: {0} {1} {2:dd-MM-yyyy HH:mm:ss.fff} {3}", client.Id, client.Name, client.ConnectTime, client.Address);
            }

            public void OnRoomListReceived(List<GameRoomAdminData> rooms)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, rooms);
                Console.WriteLine("Rooms: {0}", rooms == null ? 0 : rooms.Count);
                if (rooms != null)
                    foreach (GameRoomAdminData room in rooms)
                    {
                        Console.WriteLine("Room: {0} {1} {2}", room.Id, room.Name, room.Rule);
                        Console.WriteLine("\tClients: {0}", room.Clients == null ? 0 : room.Clients.Count);
                        if (room.Clients != null)
                            foreach (ClientAdminData client in room.Clients)
                                Console.WriteLine("\tClient: {0} {1} {2:dd-MM-yyyy HH:mm:ss.fff} {3}", client.Id, client.Name, client.ConnectTime, client.Address);
                    }
            }

            public void OnBannedListReceived(List<BanEntryData> entries)
            {
                UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, entries);
                Console.WriteLine("Banned: {0}", entries == null ? 0 : entries.Count);
                if (entries != null)
                    foreach(BanEntryData ban in entries)
                        Console.WriteLine("Ban: {0} {1} {2}", ban.Name, ban.Address, ban.Reason);
            }

            #endregion
        }
    }
}
