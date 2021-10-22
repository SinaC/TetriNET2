using System;
using System.Collections.Generic;
using TetriNET2.Client.Interfaces;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Client.ConsoleApp.UI
{
    public class ConsoleUI
    {
        private readonly IClient _client;

        public ConsoleUI(IClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));

            // TODO: _client.ConnectionLost

            _client.Connected += OnConnected;
            _client.Disconnected += OnDisconnected;

            _client.ServerStopped += OnServerStopped;

            _client.GameListReceived += OnGameListReceived;
            _client.ClientListReceived += OnClientListReceived;
            _client.GameClientListReceived += OnGameClientListReceived;

            _client.ClientConnected += OnClientConnected;
            _client.ClientDisconnected += OnClientDisconnected;
            _client.ClientGameCreated += OnClientGameCreated;
            _client.ServerGameCreated += OnServerGameCreated;
            _client.ServerGameDeleted += OnServerGameDeleted;

            _client.ServerMessageReceived += OnServerMessageReceived;
            _client.BroadcastMessageReceived += OnBroadcastMessageReceived;
            _client.PrivateMessageReceived += OnPrivateMessageReceived;
            _client.TeamChanged += OnTeamChanged;
            _client.GameCreated += OnGameCreated;

            _client.GameJoined += OnGameJoined;
            _client.GameLeft += OnGameLeft;
            _client.ClientGameJoined += OnClientGameJoined;
            _client.ClientGameLeft += OnClientGameLeft;
            _client.GameMasterModified += OnGameMasterModified;
            _client.GameStarted += OnGameStarted;
            _client.GamePaused += OnGamePaused;
            _client.GameResumed += OnGameResumed;
            _client.GameFinished += OnGameFinished;
            _client.WinListModified += OnWinListModified;
            _client.GameOptionsChanged += OnGameOptionsChanged;
            _client.VoteKickAsked += OnVoteKickAsked;
            _client.AchievementEarned += OnAchievementEarned;

            _client.PiecePlaced += OnPiecePlaced;
            _client.PlayerWon += OnPlayerWon;
            _client.PlayerLost += OnPlayerLost;
            _client.ServerLinesAdded += OnServerLinesAdded;
            _client.PlayerLinesAdded += OnPlayerLinesAdded;
            _client.SpecialUsed += OnSpecialUsed;
            _client.GridModified += OnGridModified;
            _client.ContinuousSpecialFinished += OnContinuousSpecialFinished;
        }

        private void DisplayClientList()
        {
            Console.WriteLine("Client list: {0}", _client.Clients.Count);
            foreach (ClientData client in _client.Clients)
                Console.WriteLine("\tClient: {0} {1} {2} {3} {4} {5} ", client.Id, client.Name, client.IsGameMaster, client.GameId, client.IsPlayer, client.IsSpectator);
        }

        private void DisplayGameList()
        {
            Console.WriteLine("Games: {0}", _client.Games.Count);
            foreach (GameData game in _client.Games)
            {
                Console.WriteLine("Game: {0} {1} {2}", game.Id, game.Name, game.Rule);
                Console.WriteLine("\tClients: {0}", game.Clients?.Count ?? 0);
                if (game.Clients != null)
                    foreach (ClientData client in game.Clients)
                        Console.WriteLine("\tClient: {0} {1} {2} {3} {4} {5} ", client.Id, client.Name, client.IsGameMaster, client.GameId, client.IsPlayer, client.IsSpectator);
            }
        }

        private void OnContinuousSpecialFinished(ClientData client, Specials special)
        {
            Console.WriteLine("OnContinuousSpecialFinished: {0} {1}", client?.Id ?? Guid.Empty, special);
        }

        private void OnGridModified(ClientData client, byte[] grid)
        {
            Console.WriteLine("OnGridModified: {0}", client?.Id ?? Guid.Empty);
        }

        private void OnSpecialUsed(ClientData client, ClientData target, int specialId, Specials special)
        {
            Console.WriteLine("OnSpecialUsed: {0} {1} {2} {3}", client?.Id ?? Guid.Empty, target?.Id ?? Guid.Empty, specialId, special);
        }

        private void OnPlayerLinesAdded(ClientData client, int specialId, int count)
        {
            Console.WriteLine("OnPlayerLinesAdded: {0} {1} {2}", client?.Id ?? Guid.Empty, specialId, count);
        }

        private void OnServerLinesAdded(int count)
        {
            Console.WriteLine("OnServerLinesAdded: {0}", count);
        }

        private void OnPlayerLost(ClientData client)
        {
            Console.WriteLine("OnPlayerLost: {0}", client?.Id ?? Guid.Empty);
        }

        private void OnPlayerWon(ClientData client)
        {
            Console.WriteLine("OnPlayerWon: {0}", client?.Id ?? Guid.Empty);
        }

        private void OnPiecePlaced(int firstIndex, List<Pieces> nextPieces)
        {
            Console.WriteLine("OnPiecePlaced: {0} {1}", firstIndex, nextPieces?.Count ?? 0);
        }

        private void OnAchievementEarned(ClientData client, int achievementId, string achievementTitle)
        {
            Console.WriteLine("OnAchievementEarned: {0} {1} {2}", client?.Id ?? Guid.Empty, achievementId, achievementTitle);
        }

        private void OnVoteKickAsked(ClientData sourceClient, ClientData targetClient, string reason)
        {
            Console.WriteLine("OnVoteKickAsked: {0} {1} {2}", sourceClient?.Id ?? Guid.Empty, targetClient?.Id ?? Guid.Empty, reason);
        }

        private void OnGameOptionsChanged(GameOptions gameOptions)
        {
            Console.WriteLine("OnGameOptionsChanged");
        }

        private void OnWinListModified(List<WinEntry> winEntries)
        {
            Console.WriteLine("OnWinListModified");
        }

        private void OnGameFinished(GameFinishedReasons reason, GameStatistics statistics)
        {
            Console.WriteLine("OnGameFinished: {0}", reason);
        }

        private void OnGameResumed()
        {
            Console.WriteLine("OnGameResumed");
        }

        private void OnGamePaused()
        {
            Console.WriteLine("OnGamePaused");
        }

        private void OnGameStarted()
        {
            Console.WriteLine("OnGameStarted");
        }

        private void OnGameMasterModified(ClientData client)
        {
            Console.WriteLine("OnGameMasterModified: {0}", client?.Id ?? Guid.Empty);
        }

        private void OnClientGameLeft(ClientData client)
        {
            Console.WriteLine("OnClientGameLeft: {0}", client?.Id ?? Guid.Empty);
        }

        private void OnClientGameJoined(ClientData client, bool asSpectator)
        {
            Console.WriteLine("OnClientGameJoined: {0} {1}", client?.Id ?? Guid.Empty, asSpectator);
        }

        private void OnGameLeft()
        {
            Console.WriteLine("OnGameLeft");
        }

        private void OnGameJoined(GameJoinResults result, GameData game, bool isGameMaster)
        {
            Console.WriteLine("OnGameJoined: {0} {1} {2}", result, game?.Id ?? Guid.Empty, isGameMaster);
        }

        private void OnGameCreated(GameCreateResults result, GameData game)
        {
            Console.WriteLine("OnGameCreated: {0} {1} {2} {3}", result, game?.Id ?? Guid.Empty, game?.Name ?? string.Empty, game?.Rule ?? GameRules.Custom);
        }

        private void OnTeamChanged(ClientData client, string team)
        {
            Console.WriteLine("OnTeamChanged: {0} {1}", client?.Id ?? Guid.Empty, team);
        }

        private void OnPrivateMessageReceived(ClientData client, string message)
        {
            Console.WriteLine("OnPrivateMessageReceived: {0} {1}", client?.Id ?? Guid.Empty, message);
        }

        private void OnBroadcastMessageReceived(ClientData client, string message)
        {
            Console.WriteLine("OnBroadcastMessageReceived: {0} {1}", client?.Id ?? Guid.Empty, message);
        }

        private void OnServerMessageReceived(string message)
        {
            Console.WriteLine("OnServerMessageReceived: {0}", message);
        }

        private void OnServerGameDeleted(GameData game)
        {
            Console.WriteLine("OnGameDeleted: {0}", game?.Id ?? Guid.Empty);

            DisplayGameList();
        }

        private void OnServerGameCreated(GameData game)
        {
            Console.WriteLine("OnServerGameCreated: {0} {1} {2}", game?.Id ?? Guid.Empty, game?.Name ?? string.Empty, game?.Rule ?? GameRules.Custom);
            if (game?.Clients != null)
            {
                Console.WriteLine("\tClients: {0}", game.Clients.Count);
                foreach (ClientData client in game.Clients)
                    Console.WriteLine("\tClient: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsGameMaster, client.IsPlayer, client.IsSpectator);
            }

            DisplayGameList();
        }

        private void OnClientGameCreated(ClientData client, GameData game)
        {
            Console.WriteLine("OnClientGameCreated: {0} {1} {2} {3}", client?.Id ?? Guid.Empty, game?.Id ?? Guid.Empty, game?.Name ?? string.Empty, game?.Rule ?? GameRules.Custom);
            if (game?.Clients != null)
            {
                Console.WriteLine("\tClients: {0}", game.Clients.Count);
                foreach (ClientData gameClient in game.Clients)
                    Console.WriteLine("\tClient: {0} {1} {2} {3} {4} {5}", gameClient.Id, gameClient.Name, gameClient.GameId, gameClient.IsGameMaster, gameClient.IsPlayer, gameClient.IsSpectator);
            }

            DisplayGameList();
        }

        private void OnClientDisconnected(ClientData client, LeaveReasons reason)
        {
            Console.WriteLine("OnClientDisconnected: {0} {1}", client?.Id ?? Guid.Empty, reason);

            DisplayClientList();
        }

        private void OnClientConnected(ClientData client, string name, string team)
        {
            Console.WriteLine("OnClientConnected: {0} {1} {2}", client?.Id ?? Guid.Empty, name, team);

            DisplayClientList();
        }

        private void OnGameClientListReceived(List<ClientData> clients)
        {
            Console.WriteLine("Clients in game: {0}", clients?.Count ?? 0);
            if (clients != null)
                foreach (ClientData client in clients)
                    Console.WriteLine("Client: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsGameMaster, client.IsPlayer, client.IsSpectator);
        }

        private void OnClientListReceived(List<ClientData> clients)
        {
            Console.WriteLine("Clients: {0}", clients?.Count ?? 0);
            if (clients != null)
                foreach (ClientData client in clients)
                    Console.WriteLine("Client: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsGameMaster, client.IsPlayer, client.IsSpectator);
        }

        private void OnGameListReceived(List<GameData> games)
        {
            Console.WriteLine("Games: {0}", games?.Count ?? 0);
            if (games != null)
                foreach (GameData game in games)
                {
                    Console.WriteLine("Game: {0} {1} {2}", game.Id, game.Name, game.Rule);
                    Console.WriteLine("\tClients: {0}", game.Clients?.Count ?? 0);
                    if (game.Clients != null)
                        foreach (ClientData client in game.Clients)
                            Console.WriteLine("\tClient: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsGameMaster, client.IsPlayer, client.IsSpectator);
                }
        }

        private void OnServerStopped()
        {
            Console.WriteLine("OnServerStopped");
        }

        private void OnDisconnected()
        {
            Console.WriteLine("OnDisconnected");
        }

        private void OnConnected(ConnectResults result, Versioning serverVersion, ClientData client, List<GameData> games)
        {
            Console.WriteLine("OnConnected: {0} {1}.{2} {3}", result, serverVersion?.Major ?? -1, serverVersion?.Minor ?? -1, client?.Id ?? Guid.Empty);
            Console.WriteLine("Game list: {0}", games?.Count ?? 0);
            if (games != null)
                foreach (GameData game in games)
                {
                    Console.WriteLine("Game: {0} {1} {2}", game.Id, game.Name, game.Rule);
                    Console.WriteLine("\tClients: {0}", game.Clients?.Count ?? 0);
                    if (game.Clients != null)
                        foreach (ClientData gameClient in game.Clients)
                            Console.WriteLine("\tClient: {0} {1} {2} {3} {4} {5}", gameClient.Id, gameClient.Name, gameClient.GameId, gameClient.IsGameMaster, gameClient.IsPlayer, gameClient.IsSpectator);
                }
        }
    }

    //public class ConsoleUI
    //{
    //    private const int BoardStartX = 0;
    //    private const int BoardStartY = 3;

    //    private readonly object _lock = new object();
    //    private readonly IClient _client;

    //    private bool _immunity;

    //    public ConsoleUI(IClient client)
    //    {
    //        if (client == null)
    //            throw new ArgumentNullException("client");

    //        _client = client;
    //        _client.GameStarted += OnGameStarted;
    //        _client.GameFinished += OnGameFinished;
    //        _client.Redraw += OnRedraw;
    //        _client.RedrawBoard += OnRedrawBoard;
    //        _client.PieceMoving += OnPieceMoving;
    //        _client.PieceMoved += OnPieceMoved;
    //        _client.RegisteredAsPlayer += OnRegisteredAsPlayer;
    //        _client.WinListModified += OnWinListModified;
    //        _client.PlayerLost += OnPlayerLost;
    //        _client.PlayerWon += OnPlayerWon;
    //        _client.ClientGameJoined += OnClientGameJoined;
    //        _client.ClientGameLeft += OnClientGameLeft;
    //        _client.PlayerPublishMessage += OnPlayerPublishMessage;
    //        _client.ServerPublishMessage += OnServerPublishMessage;
    //        _client.InventoryChanged += OnInventoryChanged;
    //        _client.RoundStarted += OnRoundStarted;
    //        _client.RoundFinished += OnRoundFinished;
    //        _client.LinesClearedChanged += OnLinesClearedChanged;
    //        _client.LevelChanged += OnLevelChanged;
    //        _client.ScoreChanged += OnScoreChanged;
    //        _client.SpecialUsed += OnSpecialUsed;
    //        _client.PlayerAddLines += OnPlayerAddLines;
    //        _client.ContinuousEffectToggled += OnContinuousEffectToggled;
    //        _client.AchievementEarned += OnAchievementEarned;

    //        Console.SetWindowSize(80, 30);
    //        Console.BufferWidth = 80;
    //        Console.BufferHeight = 30;
    //    }

    //    private void OnAchievementEarned(ClientData player, int achievementId, string achievementTitle)
    //    {
    //       lock (_lock)
    //       {
    //           Console.ResetColor();
    //           Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 0);
    //           Console.Write("You have earned [{0}]", achievementTitle);
    //       }
    //    }

    //    private void OnPlayerAddLines(int playerId, string playerName, int specialId, int count)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 8 + (specialId%10));
    //            Console.Write("{0}. {1} line{2} added to All from {3}", specialId, count, (count > 1) ? "s" : "", playerName );
    //        }
    //    }

    //    private void OnSpecialUsed(ClientData player, ClientData target, int specialId, Specials special)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 8 + (specialId%10));
    //            Console.Write("{0}. {1} on {2} from {3}", specialId, GetSpecialString(special), target.Name, player.Name);
    //        }
    //    }

    //    private void OnLevelChanged(int level)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 6);
    //            Console.Write("Level: {0}", level == 0 ? _client.Level : level);
    //        }
    //    }

    //    private void OnLinesClearedChanged(int linesCleared)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 5);
    //            Console.Write("#Lines cleared: {0}", linesCleared == 0 ? _client.LinesCleared : linesCleared);
    //        }
    //    }


    //    private void OnScoreChanged(int score)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 7);
    //            Console.Write("Score: {0:#,0}", score == 0 ? _client.Score : score);
    //        }
    //    }

    //    private void OnRoundFinished(int deletedRows)
    //    {
    //        HideNextPieceColor();
    //    }

    //    private void OnRoundStarted()
    //    {
    //        DisplayNextPieceColor();
    //    }

    //    private void OnInventoryChanged()
    //    {
    //        DisplayInventory();
    //    }

    //    private void OnGameFinished(GameFinishedReasons reason, GameStatistics statistics)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 4);
    //            Console.Write("Game finished");
    //        }
    //    }

    //    private void OnGameStarted()
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 4);
    //            Console.Write("Game started");
    //        }

    //        _immunity = false;

    //        OnRedraw();
    //        DisplayNextPieceColor();
    //        OnLinesClearedChanged(0);
    //        OnLevelChanged(0);
    //        OnScoreChanged(0);
    //    }

    //    private void OnServerPublishMessage(string msg)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 0);
    //            Console.Write("SERVER: {0}", msg);
    //        }
    //    }

    //    private void OnPlayerPublishMessage(string playerName, string msg)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 0);
    //            Console.Write("{0}: {1}", playerName, msg);
    //        }
    //    }

    //    private void OnClientGameLeft(ClientData player)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 2);
    //            Console.Write("{0} [{1}] left", player.Name, player.Id);
    //        }
    //    }

    //    private void OnClientGameJoined(ClientData player, bool asSpectator)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 2);
    //            Console.Write("{0}|{1} [{2}] joined", player.Name, player.Team, player.Id);
    //        }
    //    }

    //    private void OnRegisteredAsPlayer(RegistrationResults result, Versioning serverVersion, int playerId, bool isServerMaster)
    //    {
    //        if (result == RegistrationResults.RegistrationSuccessful)
    //        {
    //            lock (_lock)
    //            {
    //                Console.ResetColor();
    //                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 1);
    //                Console.Write("Registration succeeded -> {0}", playerId);
    //            }
    //        }
    //        else
    //        {
    //            lock (_lock)
    //            {
    //                Console.ResetColor();
    //                Console.SetCursorPosition(60, 1);
    //                Console.Write("Registration failed!!! {0}. ServerVersion: {1}.{2}", result, serverVersion.Major, serverVersion.Minor);
    //            }
    //        }
    //    }

    //    private void OnPlayerWon(ClientData player)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 21);
    //            Console.Write("The winner is {0}", player.Name);
    //        }
    //    }

    //    private void OnPlayerLost(ClientData player)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 20);
    //            Console.Write("Player {0} has lost", player.Name);
    //        }
    //    }

    //    private void OnWinListModified(List<WinEntry> winList)
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            // Display only top 5
    //            winList.Sort((entry1, entry2) => entry2.Score.CompareTo(entry1.Score)); // descending
    //            for (int i = 0; i < (winList.Count > 5 ? 5 : winList.Count); i++)
    //            {
    //                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 22 + i);
    //                Console.Write("{0}[{1}]:{2}", winList[i].PlayerName, winList[i].Team, winList[i].Score);
    //            }
    //        }
    //    }

    //    private void OnContinuousEffectToggled(Specials special, bool active, double durationLeftInSeconds)
    //    {
    //        if (special == Specials.Immunity)
    //        {
    //            _immunity = active;
    //            DisplayBoardColor();
    //        }
    //    }

    //    private void OnPieceMoved()
    //    {
    //        DisplayCurrentPieceColor();
    //    }

    //    private void OnPieceMoving()
    //    {
    //        HideCurrentPieceColor();
    //    }

    //    private void OnRedrawBoard(int playerId, IBoard board)
    //    {
    //        // NOP
    //    }

    //    private void OnRedraw()
    //    {
    //        // Board
    //        DisplayBoardColor();
    //        // Piece
    //        DisplayCurrentPieceColor();
    //        // Inventory
    //        DisplayInventory();
    //    }

    //    private void DisplayBoardColor()
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            for (int y = _client.Board.Height; y >= 1; y--)
    //            {
    //                Console.SetCursorPosition(0 + BoardStartX, _client.Board.Height - y + BoardStartY);
    //                Console.Write(_immunity ? "*" : "|");

    //                for (int x = 1; x <= _client.Board.Width; x++)
    //                {
    //                    Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
    //                    byte cellValue = _client.Board[x, y];
    //                    if (cellValue == CellHelper.EmptyCell)
    //                        Console.Write(".");
    //                    else
    //                    {
    //                        Pieces cellPiece = CellHelper.GetColor(cellValue);
    //                        Console.BackgroundColor = GetPieceColor(cellPiece);
    //                        Specials cellSpecial = CellHelper.GetSpecial(cellValue);
    //                        if (cellSpecial == Specials.Invalid)
    //                            Console.Write(" ");
    //                        else
    //                        {
    //                            Console.ForegroundColor = ConsoleColor.Black;
    //                            Console.Write(ConvertSpecial(cellSpecial));
    //                        }
    //                        Console.ResetColor();
    //                    }
    //                }
    //                Console.SetCursorPosition(_client.Board.Width + 1 + BoardStartX, _client.Board.Height - y + BoardStartY);
    //                Console.Write(_immunity ? "*" : "|");
    //            }
    //            Console.SetCursorPosition(0 + BoardStartX, _client.Board.Height + BoardStartY);
    //            Console.Write("".PadLeft(_client.Board.Width + 2, _immunity ? '*' : '-'));
    //        }
    //    }

    //    private void DisplayCurrentPieceColor()
    //    {
    //        lock (_lock)
    //        {
    //            // draw current piece
    //            if (_client.CurrentPiece != null)
    //            {
    //                Pieces cellPiece = _client.CurrentPiece.Value;
    //                Console.BackgroundColor = GetPieceColor(cellPiece);
    //                for (int i = 1; i <= _client.CurrentPiece.TotalCells; i++)
    //                {
    //                    int x, y;
    //                    _client.CurrentPiece.GetCellAbsolutePosition(i, out x, out y);
    //                    if (x >= 0 && x <= _client.Board.Width && y >= 0 && y <= _client.Board.Height)
    //                    {
    //                        Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
    //                        Console.Write(" ");
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    private void HideCurrentPieceColor()
    //    {
    //        lock (_lock)
    //        {
    //            // hide current piece
    //            if (_client.CurrentPiece != null)
    //            {
    //                Console.ResetColor();
    //                for (int i = 1; i <= _client.CurrentPiece.TotalCells; i++)
    //                {
    //                    int x, y;
    //                    _client.CurrentPiece.GetCellAbsolutePosition(i, out x, out y);
    //                    if (x >= 0 && x <= _client.Board.Width && y >= 0 && y <= _client.Board.Height)
    //                    {
    //                        Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
    //                        Console.Write(".");
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    private void DisplayNextPieceColor()
    //    {
    //        lock (_lock)
    //        {
    //            // draw next piece
    //            if (_client.NextPiece != null)
    //            {
    //                IPiece temp = _client.NextPiece.Clone();
    //                int minX, minY, maxX, maxY;
    //                temp.GetAbsoluteBoundingRectangle(out minX, out minY, out maxX, out maxY);
    //                // Move to top, left
    //                temp.Translate(-minX, 0);
    //                if (maxY > _client.Board.Height)
    //                    temp.Translate(0, _client.Board.Height - maxY);
    //                // Display piece
    //                Pieces cellPiece = temp.Value;
    //                Console.BackgroundColor = GetPieceColor(cellPiece);
    //                for (int i = 1; i <= temp.TotalCells; i++)
    //                {
    //                    int x, y;
    //                    temp.GetCellAbsolutePosition(i, out x, out y);
    //                    Console.SetCursorPosition(x, _client.Board.Height - y);
    //                    Console.Write(" ");
    //                }
    //            }
    //        }
    //    }

    //    private void HideNextPieceColor()
    //    {
    //        lock (_lock)
    //        {
    //            // hide next piece
    //            if (_client.NextPiece != null)
    //            {
    //                Console.ResetColor();
    //                IPiece temp = _client.NextPiece.Clone();
    //                int minX, minY, maxX, maxY;
    //                temp.GetAbsoluteBoundingRectangle(out minX, out minY, out maxX, out maxY);
    //                // Move to top, left
    //                temp.Translate(-minX, 0);
    //                if (maxY > _client.Board.Height)
    //                    temp.Translate(0, _client.Board.Height - maxY);
    //                // hide piece
    //                for (int i = 1; i <= temp.TotalCells; i++)
    //                {
    //                    int x, y;
    //                    temp.GetCellAbsolutePosition(i, out x, out y);
    //                    Console.SetCursorPosition(x, _client.Board.Height - y);
    //                    Console.Write(" ");
    //                }
    //            }
    //        }
    //    }

    //    //private void DisplayBoardNoColor()
    //    //{
    //    //    lock (_lock)
    //    //    {
    //    //        for (int y = _client.Board.Height; y >= 1; y--)
    //    //        {
    //    //            StringBuilder sb = new StringBuilder("|");
    //    //            for (int x = 1; x <= _client.Board.Width; x++)
    //    //            {
    //    //                byte cellValue = _client.Board[x, y];
    //    //                if (cellValue == CellHelper.EmptyCell)
    //    //                    sb.Append(".");
    //    //                else {
    //    //                    Pieces cellPiece = CellHelper.GetColor(cellValue);
    //    //                    Specials cellSpecial = CellHelper.GetSpecial(cellValue);
    //    //                    if (cellSpecial == Specials.Invalid)
    //    //                        sb.Append((int) cellPiece);
    //    //                    else
    //    //                        sb.Append(ConvertSpecial(cellSpecial));
    //    //                }
    //    //            }
    //    //            sb.Append("|");
    //    //            Console.SetCursorPosition(0 + BoardStartX, _client.Board.Height - y + BoardStartY);
    //    //            Console.Write(sb.ToString());
    //    //        }
    //    //        Console.SetCursorPosition(0 + BoardStartX, _client.Board.Height + BoardStartY);
    //    //        Console.Write("".PadLeft(_client.Board.Width + 2, '-'));
    //    //    }
    //    //}

    //    //private void DisplayCurrentPieceNoColor()
    //    //{
    //    //    lock (_lock)
    //    //    {
    //    //        // draw current piece
    //    //        if (_client.CurrentPiece != null)
    //    //        {
    //    //            for (int i = 1; i <= _client.CurrentPiece.TotalCells; i++)
    //    //            {
    //    //                int x, y;
    //    //                _client.CurrentPiece.GetCellAbsolutePosition(i, out x, out y);
    //    //                if (x >= 0 && x <= _client.Board.Width && y >= 0 && y <= _client.Board.Height)
    //    //                {
    //    //                    Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
    //    //                    Console.Write(_client.CurrentPiece.Value);
    //    //                }
    //    //            }
    //    //        }
    //    //    }
    //    //}

    //    //private void HideCurrentPieceNoColor()
    //    //{
    //    //    lock (_lock)
    //    //    {
    //    //        // draw current piece
    //    //        if (_client.CurrentPiece != null)
    //    //        {
    //    //            for (int i = 1; i <= _client.CurrentPiece.TotalCells; i++)
    //    //            {
    //    //                int x, y;
    //    //                _client.CurrentPiece.GetCellAbsolutePosition(i, out x, out y);
    //    //                if (x >= 0 && x <= _client.Board.Width && y >= 0 && y <= _client.Board.Height)
    //    //                {
    //    //                    Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
    //    //                    Console.Write(".");
    //    //                }
    //    //            }
    //    //        }
    //    //    }
    //    //}

    //    private void DisplayInventory()
    //    {
    //        lock (_lock)
    //        {
    //            Console.ResetColor();
    //            IReadOnlyCollection<Specials> specials = _client.Inventory;
    //            StringBuilder sb2 = new StringBuilder();
    //            int i = 0;
    //            foreach(Specials special in specials)
    //            {
    //                if (i == 0)
    //                    sb2.Append(string.Format("[{0}]", ConvertSpecial(special)));
    //                else
    //                    sb2.Append(ConvertSpecial(special));
    //                i++;
    //            }
    //            Console.SetCursorPosition(0, _client.Board.Height + 1 + BoardStartY);
    //            Console.Write(sb2.ToString().PadRight(20, ' '));
    //        }
    //    }

    //    private ConsoleColor GetPieceColor(Pieces piece)
    //    {
    //        switch (piece)
    //        {
    //            case Pieces.TetriminoI:
    //                return ConsoleColor.Blue;
    //            case Pieces.TetriminoJ:
    //                return ConsoleColor.Green;
    //            case Pieces.TetriminoL:
    //                return ConsoleColor.Magenta;
    //            case Pieces.TetriminoO:
    //                return ConsoleColor.Yellow;
    //            case Pieces.TetriminoS:
    //                return ConsoleColor.Blue;
    //            case Pieces.TetriminoT:
    //                return ConsoleColor.Yellow;
    //            case Pieces.TetriminoZ:
    //                return ConsoleColor.Red;
    //        }
    //        return ConsoleColor.Gray;
    //    }

    //    private char ConvertSpecial(Specials special)
    //    {
    //        SpecialAttribute attribute = EnumHelper.GetAttribute<SpecialAttribute>(special);
    //        return attribute == null ? '?' : attribute.ShortName;
    //    }

    //    private string GetSpecialString(Specials special)
    //    {
    //        SpecialAttribute attribute = EnumHelper.GetAttribute<SpecialAttribute>(special);
    //        return attribute == null ? special.ToString() : attribute.LongName;
    //    }
    //}
}