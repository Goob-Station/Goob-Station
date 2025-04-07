using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared._durkcode.ServerCurrency;
using Robust.Server.Player;
using Robust.Shared.Asynchronous;
using Robust.Shared.Network;

namespace Content.Server._durkcode.ServerCurrency
{
    public sealed class ServerCurrencyManager
    {
        [Dependency] private readonly IServerDbManager _db = default!;
        [Dependency] private readonly ITaskManager _task = default!;
        [Dependency] private readonly IPlayerManager _player = default!;
        private readonly List<Task> _pendingSaveTasks = new();
        public event Action<PlayerBalanceChangeEvent>? BalanceChange;
        private ISawmill _sawmill = default!;

        public void Initialize()
        {
            _sawmill = Logger.GetSawmill("server_currency");
        }

        /// <summary>
        /// Saves player balances to the database before allowing the server to shutdown.
        /// </summary>
        public void Shutdown()
        {
            _task.BlockWaitOnTask(Task.WhenAll(_pendingSaveTasks));
        }

        /// <summary>
        /// Checks if a player has enough currency to purchase something.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of currency needed.</param>
        /// <param name="balance">The player's balance.</param>
        /// <returns>Returns true if the player has enough in their balance.</returns>
        public bool CanAfford(NetUserId userId, int amount, out int balance)
        {
            balance = GetBalance(userId);
            return balance >= amount && balance - amount >= 0;
        }

        /// <summary>
        /// Converts an integer to a string representing the count followed by the appropriate currency localization (singular or plural) defined in server_currency.ftl.
        /// Useful for displaying balances and prices.
        /// </summary>
        /// <param name="amount">The amount of currency to display.</param>
        /// <returns>A string containing the count and the correct form of the currency name.</returns>
        public string Stringify(int amount) => amount == 1
            ? $"{amount} {Loc.GetString("server-currency-name-singular")}"
            : $"{amount} {Loc.GetString("server-currency-name-plural")}";

        /// <summary>
        /// Adds currency to a player.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of currency to add.</param>
        /// <returns>An integer containing the new amount of currency attributed to the player.</returns>
        public int AddCurrency(NetUserId userId, int amount)
        {
            var newBalance = ModifyBalance(userId, amount);
            _sawmill.Info($"Added {amount} currency to {userId} account. Current balance: {newBalance}");
            return newBalance;
        }

        /// <summary>
        /// Removes currency from a player.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of currency to remove.</param>
        /// <returns>An integer containing the new amount of currency attributed to the player.</returns>
        public int RemoveCurrency(NetUserId userId, int amount)
        {
            var newBalance = ModifyBalance(userId, -amount);
            _sawmill.Info($"Removed {amount} currency from {userId} account. Current balance: {newBalance}");
            return newBalance;
        }

        /// <summary>
        /// Transfers currency from player to another player.
        /// </summary>
        /// <param name="sourceUserId">The source player's NetUserId</param>
        /// <param name="targetUserId">The target player's NetUserId</param>
        /// <param name="amount">The amount of currency to add.</param>
        /// <returns>A pair of integers containing the new amount of currencies attributed to the players</returns>
        /// <remarks>Purely convenience function, but lessens log load</remarks>
        public (int, int) TransferCurrency(NetUserId sourceUserId, NetUserId targetUserId, int amount)
        {
            var newAccountValues = (ModifyBalance(sourceUserId, -amount), ModifyBalance(targetUserId, amount));
            _sawmill.Info($"Transferring {amount} currency from {sourceUserId} to {targetUserId}. Current balances: {newAccountValues.Item1}, {newAccountValues.Item2}");
            return newAccountValues;
        }

        /// <summary>
        /// Sets a player's balance.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of currency that will be set.</param>
        /// <returns>An integer containing the old amount of currency attributed to the player.</returns>
        /// <remarks>Use the return value instead of calling <see cref="GetBalance(NetUserId)"/> prior to this.</remarks>
        public int SetBalance(NetUserId userId, int amount)
        {
            var oldBalance = Task.Run(() => SetBalanceAsync(userId, amount)).GetAwaiter().GetResult();
            if (_player.TryGetSessionById(userId, out var userSession))
                BalanceChange?.Invoke(new PlayerBalanceChangeEvent(userSession, userId, amount, oldBalance));
            _sawmill.Info($"Setting {userId} account balance to {amount} from {oldBalance}");
            return oldBalance;
        }

        /// <summary>
        /// Gets a player's balance.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <returns>The players balance.</returns>
        public int GetBalance(NetUserId userId)
        {
            return Task.Run(() => GetBalanceAsync(userId)).GetAwaiter().GetResult();
        }

        #region Internal/Async tasks

        /// <summary>
        /// Modifies a player's balance.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amountDelta">The amount of currency that will be set.</param>
        /// <returns>An integer containing the new amount of currency attributed to the player.</returns>
        /// <remarks>Use the return value instead of calling <see cref="GetBalance(NetUserId)"/> after to this.</remarks>
        private int ModifyBalance(NetUserId userId, int amountDelta)
        {
            var result = Task.Run(() => ModifyBalanceAsync(userId, amountDelta)).GetAwaiter().GetResult();
            if (_player.TryGetSessionById(userId, out var userSession))
                BalanceChange?.Invoke(new PlayerBalanceChangeEvent(userSession, userId, result, result - amountDelta));
            return result;
        }

        /// <summary>
        /// Sets a player's balance.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of currency that will be set.</param>
        /// <param name="oldAmount">The amount of currency that will be set.</param>
        /// <remarks>This and its calees will block server shutdown until execution finishes.</remarks>
        private async Task SetBalanceAsyncInternal(NetUserId userId, int amount, int oldAmount)
        {
            var task = Task.Run(() => _db.SetServerCurrency(userId, amount));
            TrackPending(task);
            await task;
        }

        /// <summary>
        /// Sets a player's balance.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of currency that will be set.</param>
        /// <returns>An integer containing the old amount of currency attributed to the player.</returns>
        /// <remarks>Use the return value instead of calling <see cref="GetBalance(NetUserId)"/> prior to this.</remarks>
        private async Task<int> SetBalanceAsync(NetUserId userId, int amount)
        {
            // We need to block it first to ensure we don't read our own amount, hence sync function
            var oldAmount = GetBalance(userId);
            await SetBalanceAsyncInternal(userId, amount, oldAmount);
            return oldAmount;
        }

        /// <summary>
        /// Gets a player's balance.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <returns>An integer containing the amount of currency attributed to the player.</returns>
        private async Task<int> GetBalanceAsync(NetUserId userId) => await _db.GetServerCurrency(userId);

        /// <summary>
        /// Modifies a player's balance.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amountDelta">The amount of currency that will be given or taken.</param>
        /// <returns>An integer containing the new amount of currency attributed to the player.</returns>
        /// <remarks>This and its calees will block server shutdown until execution finishes.</remarks>
        private async Task<int> ModifyBalanceAsync(NetUserId userId, int amountDelta)
        {
            var task = Task.Run(() => _db.ModifyServerCurrency(userId, amountDelta));
            TrackPending(task);
            return await task;
        }

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
