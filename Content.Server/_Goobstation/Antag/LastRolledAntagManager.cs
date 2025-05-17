// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Database;
using Robust.Server.Player;
using Robust.Shared.Asynchronous;
using Robust.Shared.Network;
using System;
using System.Threading.Tasks;

// has to be in Content.Server to exist
namespace Content.Server._Goobstation.Antag
{
    public sealed class LastRolledAntagManager
    {
        [Dependency] private readonly IServerDbManager _db = default!;
        [Dependency] private readonly ITaskManager _task = default!;
        [Dependency] private readonly IPlayerManager _player = default!;
        private readonly List<Task> _pendingSaveTasks = new();
        private ISawmill _sawmill = default!;

        public void Initialize()
        {
            _sawmill = Logger.GetSawmill("last_antag");
        }

        /// <summary>
        /// Saves last rolled values to the database before allowing the server to shutdown.
        /// </summary>
        public void Shutdown()
        {
            _task.BlockWaitOnTask(Task.WhenAll(_pendingSaveTasks));
        }

        /// <summary>
        /// Sets a player's last rolled antag time.
        /// </summary>
        public TimeSpan SetLastRolled(NetUserId userId, TimeSpan to)
        {
            var oldTime = Task.Run(() => SetTimeAsync(userId, to)).GetAwaiter().GetResult();
            _sawmill.Info($"Setting {userId} last rolled antag to {to} from {oldTime}");
            return oldTime;
        }

        /// <summary>
        /// Gets a player's last rolled antag time.
        /// </summary>
        public TimeSpan GetLastRolled(NetUserId userId)
        {
            return Task.Run(() => GetTimeAsync(userId)).GetAwaiter().GetResult();
        }

        #region Internal/Async tasks

        /// <summary>
        /// Sets a player's last rolled antag time.
        /// </summary>
        private async Task SetTimeAsyncInternal(NetUserId userId, TimeSpan time, TimeSpan oldTime)
        {
            Task<bool> setTimeTask = _db.SetLastRolledAntag(userId, time);
            TrackPending(setTimeTask); // Track the Task<bool>
            bool success = await setTimeTask;

            if (success)
                _sawmill.Debug($"Successfully set LastRolledAntag for {userId} from {oldTime} to {time}");
            else
                _sawmill.Error($"Failed to set LastRolledAntag for {userId}. Player not found or other issue.");
        }

        /// <summary>
        /// Sets a player's last rolled antag time.
        /// </summary>
        private async Task<TimeSpan> SetTimeAsync(NetUserId userId, TimeSpan to)
        {
            var oldTime = GetLastRolled(userId);
            await SetTimeAsyncInternal(userId, to, oldTime);
            return oldTime;
        }

        /// <summary>
        /// Gets a player's last rolled antag time.
        /// </summary>
        private async Task<TimeSpan> GetTimeAsync(NetUserId userId) => await _db.GetLastRolledAntag(userId);

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

        #endregion
    }
}
