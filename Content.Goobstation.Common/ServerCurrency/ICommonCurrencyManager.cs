// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading.Tasks;
using Robust.Shared.Asynchronous;
using Robust.Shared.Network;

namespace Content.Goobstation.Common.ServerCurrency;

public interface ICommonCurrencyManager
{
    public event Action? ClientBalanceChange;
    public event Action<PlayerBalanceChangeEvent>? BalanceChange;

    public void Initialize();

    /// <summary>
    /// Saves player balances to the database before allowing the server to shutdown.
    /// </summary>
    public void Shutdown();

    /// <summary>
    /// Checks if a player has enough currency to purchase something.
    /// </summary>
    /// <param name="userId">The player's NetUserId</param>
    /// <param name="amount">The amount of currency needed.</param>
    /// <param name="balance">The player's balance.</param>
    /// <returns>Returns true if the player has enough in their balance.</returns>
    public bool CanAfford(NetUserId? userId, int amount, out int balance);

    /// <summary>
    /// Converts an integer to a string representing the count followed by the appropriate currency localization (singular or plural) defined in server_currency.ftl.
    /// Useful for displaying balances and prices.
    /// </summary>
    /// <param name="amount">The amount of currency to display.</param>
    /// <returns>A string containing the count and the correct form of the currency name.</returns>
    public string Stringify(int amount);

    /// <summary>
    /// Adds currency to a player.
    /// </summary>
    /// <param name="userId">The player's NetUserId</param>
    /// <param name="amount">The amount of currency to add.</param>
    /// <returns>An integer containing the new amount of currency attributed to the player.</returns>
    public int AddCurrency(NetUserId userId, int amount);

    /// <summary>
    /// Removes currency from a player.
    /// </summary>
    /// <param name="userId">The player's NetUserId</param>
    /// <param name="amount">The amount of currency to remove.</param>
    /// <returns>An integer containing the new amount of currency attributed to the player.</returns>
    public int RemoveCurrency(NetUserId userId, int amount);

    /// <summary>
    /// Transfers currency from player to another player.
    /// </summary>
    /// <param name="sourceUserId">The source player's NetUserId</param>
    /// <param name="targetUserId">The target player's NetUserId</param>
    /// <param name="amount">The amount of currency to add.</param>
    /// <returns>A pair of integers containing the new amount of currencies attributed to the players</returns>
    /// <remarks>Purely convenience function, but lessens log load</remarks>
    public (int, int) TransferCurrency(NetUserId sourceUserId, NetUserId targetUserId, int amount);

    /// <summary>
    /// Sets a player's balance.
    /// </summary>
    /// <param name="userId">The player's NetUserId</param>
    /// <param name="amount">The amount of currency that will be set.</param>
    /// <returns>An integer containing the old amount of currency attributed to the player.</returns>
    /// <remarks>Use the return value instead of calling <see cref="GetBalance(NetUserId)"/> prior to this.</remarks>
    public int SetBalance(NetUserId userId, int amount);

    /// <summary>
    /// Gets a player's balance.
    /// </summary>
    /// <param name="userId">The player's NetUserId</param>
    /// <remarks>Null implies the client is calling this</remarks>
    /// <returns>The players balance.</returns>
    public int GetBalance(NetUserId? userId = null);
}
