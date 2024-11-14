using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Content.Server.Database;
using Robust.Shared.Network;

namespace Content.Server._Goobstation.ServerCurrency.Systems
{
    public sealed class ServerCurrencySystem
    {
        [Dependency] private readonly IServerDbManager _db = default!;

        /// <summary>
        /// Checks if a player has enough currency to purchase something.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of currency needed.</param>
        /// <returns>Returns true if the player has enough currency.</returns>
        public async Task<bool> CanAfford(NetUserId userId, int amount) => await GetCurrency(userId) >= amount;

        /// <summary>
        /// Converts an integer to a string representing the count followed by the appropriate currency localization (singular or plural) defined in server_currency.ftl.
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
        public async Task<int> AddCurrency(NetUserId userId, int amount) => await SetCurrency(userId, await GetCurrency(userId) + amount);

        /// <summary>
        /// Removes currency from a player.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of currency to remove.</param>
        /// <returns>An integer containing the new amount of currency attributed to the player.</returns>
        public async Task<int> RemoveCurrency(NetUserId userId, int amount) => await SetCurrency(userId, await GetCurrency(userId) - amount);

        /// <summary>
        /// Sets a player's currency.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of currency that will be set.</param>
        /// <returns>An integer containing the new amount of currency attributed to the player.</returns>
        public async Task<int> SetCurrency(NetUserId userId, int amount)
        {
            await _db.SetServerCurrency(userId, amount);
            return amount;
        }

        /// <summary>
        /// Gets a player's currency.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <returns>An integer containing the amount of currency attributed to the player.</returns>
        public async Task<int> GetCurrency(NetUserId userId) => await _db.GetServerCurrency(userId);

    }
}
