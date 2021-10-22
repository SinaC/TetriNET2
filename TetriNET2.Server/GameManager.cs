using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server
{
    public sealed  class GameManager : IGameManager
    {
        private readonly Dictionary<Guid, IGame> _games = new Dictionary<Guid, IGame>();

        public GameManager(ISettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            int maxGames = settings.MaxGames;
            if (maxGames <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxGames), "maxGames must be strictly positive");

            MaxGames = maxGames;
        }

        #region IGameManager

        public int MaxGames { get; }

        public int GameCount => _games.Count;

        public object LockObject { get; } = new object();

        public IReadOnlyCollection<IGame> Games => _games.Values.ToList();

        public IGame this[Guid guid]
        {
            get
            {
                _games.TryGetValue(guid, out var game);
                return game;
            }
        }

        public IGame this[string name]
        {
            get
            {
                KeyValuePair<Guid, IGame> kv = _games.FirstOrDefault(x => x.Value.Name == name);
                if (kv.Equals(default(KeyValuePair<Guid, IGame>)))
                    return null;
                return kv.Value;
            }
        }

        public bool Add(IGame game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            if (GameCount >= MaxGames)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Too many games");
                return false;
            }

            if (_games.ContainsKey(game.Id))
            {
                Log.Default.WriteLine(LogLevels.Warning, "{0} already added", game.Name);
                return false;
            }

            //
            _games.Add(game.Id, game);

            //
            return true;
        }

        public bool Remove(IGame game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            bool removed = _games.Remove(game.Id);
            return removed;
        }

        public void Clear()
        {
            foreach(KeyValuePair<Guid, IGame> game in _games)
                game.Value.Clear();
            _games.Clear();
        }

        #endregion
    }
}
