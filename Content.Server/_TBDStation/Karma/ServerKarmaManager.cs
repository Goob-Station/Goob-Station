using System.Threading;
using System.Threading.Tasks;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Asynchronous;
using Robust.Shared.Timing;
using Content.Server.Database;
using Robust.Server.Player;
using Content.Shared._TBDStation.ServerKarma.Events;

namespace Content.Server._TBDStation.ServerKarma
{
    public sealed class ServerKarmaManager : IPostInjectInit
    {
        [Dependency] private readonly IServerDbManager _db = default!;
        [Dependency] private readonly UserDbDataManager _userDb = default!;
        [Dependency] private readonly ITaskManager _task = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IPlayerManager _player = default!;
        private ISawmill _sawmill = default!;
        private Dictionary<NetUserId, KarmaData> _karmas = new();
        private readonly List<Task> _pendingSaveTasks = new();
        private TimeSpan _saveInterval = TimeSpan.FromSeconds(60); // Yeah, players will only gain coins mid round from gifting/admins, so we dont need to update often.
        private TimeSpan _lastSave;
        public event Action<PlayerKarmaChangeEvent>? KarmaChange;

        public void Initialize()
        {
            _sawmill = Logger.GetSawmill("server_Karma");
        }

        void IPostInjectInit.PostInject()
        {
            _userDb.AddOnLoadPlayer(PlayerConnected);
            _userDb.AddOnPlayerDisconnect(PlayerDisconnected);
        }

        /// <summary>
        /// Saves player data at set intervals.
        /// </summary>
        public void Update()
        {
            if (_timing.RealTime < _lastSave + _saveInterval)
                return;
            Save();
        }

        /// <summary>
        /// Saves player karmas to the database before allowing the server to shutdown.
        /// </summary>
        public void Shutdown()
        {
            Save();
            _task.BlockWaitOnTask(Task.WhenAll(_pendingSaveTasks));
        }

        /// <summary>
        /// Loads player's karma when they connect
        /// </summary>
        public async Task PlayerConnected(ICommonSession session, CancellationToken cancel)
        {
            var user = session.UserId;
            var data = new KarmaData();
            _karmas.Add(user, data);

            data.Karma = await GetKarmaAsync(user);
            cancel.ThrowIfCancellationRequested();

            data.Initialized = true;
        }

        /// <summary>
        /// Saves player's karma when they disconnect
        /// </summary>
        public void PlayerDisconnected(ICommonSession session)
        {
            SavePlayer(session.UserId);
            _karmas.Remove(session.UserId);
        }

        /// <summary>
        ///  Saves player karmas to the database.
        /// </summary>
        public async void Save()
        {
            foreach (var player in _karmas)
                SavePlayer(player.Key);

            _lastSave = _timing.RealTime;
        }

        /// <summary>
        /// Save karma for a player to the database.
        /// </summary>
        public async void SavePlayer(NetUserId player)
        {
            if (!_karmas[player].IsDirty)
                return;
            _karmas[player].IsDirty = false;
            TrackPending(SetKarmaAsync(player, _karmas[player].Karma));
        }

        /// <summary>
        /// Checks if a player has enough Karma to purchase something.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of Karma needed.</param>
        /// <param name="karma">The player's karma.</param>
        /// <returns>Returns true if the player has enough in their karma.</returns>
        public bool CanAfford(NetUserId userId, int amount, out int karma)
        {
            karma = GetKarma(userId);
            return karma >= amount && karma - amount >= 0;
        }

        /// <summary>
        /// Converts an integer to a string representing the count followed by the appropriate Karma localization (singular or plural) defined in server_Karma.ftl.
        /// Useful for displaying karmas and prices.
        /// </summary>
        /// <param name="amount">The amount of Karma to display.</param>
        /// <returns>A string containing the count and the correct form of the Karma name.</returns>
        public string Stringify(int amount) => amount == 1
            ? $"{amount} {Loc.GetString("server-karma-name-singular")}"
            : $"{amount} {Loc.GetString("server-karma-name-plural")}";

        /// <summary>
        /// Adds Karma to a player.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of Karma to add.</param>
        /// <returns>An integer containing the new amount of Karma attributed to the player.</returns>
        public int AddKarma(NetUserId userId, int amount) => SetKarma(userId, GetKarma(userId) + amount);

        /// <summary>
        /// Removes Karma from a player.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of Karma to remove.</param>
        /// <returns>An integer containing the new amount of Karma attributed to the player.</returns>
        public int RemoveKarma(NetUserId userId, int amount) => SetKarma(userId, GetKarma(userId) - amount);

        /// <summary>
        /// Sets a player's Karma.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of Karma that will be set.</param>
        /// <returns>An integer containing the new amount of Karma attributed to the player.</returns>
        public int SetKarma(NetUserId userId, int amount)
        {
            if (!_karmas.TryGetValue(userId, out var data) || !data.Initialized)
            {
                _sawmill.Warning($"Attempted to set karma, which was not loaded for player {userId}");
                return 0;
            }
            amount = Math.Clamp(amount, -1200, 400); // Clamp karma cannot be higher then 400 or less then -1200.

            var karmaData = _karmas[userId];

            if(_player.TryGetSessionById(userId, out var userSession))
                KarmaChange?.Invoke(new PlayerKarmaChangeEvent(userSession, userId, amount, karmaData.Karma));

            karmaData.Karma = amount;
            karmaData.IsDirty = true;
            return amount;
        }

        /// <summary>
        /// Gets a player's karma.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <returns>The players karma.</returns>
        public int GetKarma(NetUserId userId)
        {
            if (!_karmas.TryGetValue(userId, out var data) || !data.Initialized)
            {
                _sawmill.Warning($"Attempted to get karma, which was not loaded for player {userId.ToString()}");
                return 0;
            }

            return data.Karma;
        }

        /// <summary>
        /// Karma info for a particular player.
        /// </summary>
        private sealed class KarmaData
        {
            public int Karma = new();
            public bool IsDirty = false;
            public bool Initialized = false;
        }

        // Async Tasks

        /// <summary>
        /// Sets a player's karma.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of Karma that will be set.</param>
        /// <returns>An integer containing the new amount of Karma attributed to the player.</returns>
        public async Task SetKarmaAsync(NetUserId userId, int amount) => await _db.SetServerKarma(userId, amount);

        /// <summary>
        /// Gets a player's karma.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <returns>An integer containing the amount of Karma attributed to the player.</returns>
        public async Task<int> GetKarmaAsync(NetUserId userId) => await _db.GetServerKarma(userId);

        /// <summary>
        /// Track a database save task to make sure we block server shutdown on it.
        /// </summary>
        private async void TrackPending(Task task)
        {
            _pendingSaveTasks.Add(task);

            try
            {
                await task;
            }
            finally
            {
                _pendingSaveTasks.Remove(task);
            }
        }


        private const int _tbdstationKarmaPerDamage = 2;

    }
}
