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

        public async Task<int> AddCurrency(NetUserId userId, int currency) => await SetCurrency(userId, await GetCurrency(userId) + currency);
        public async Task<int> SubtractCurrency(NetUserId userId, int currency) => await SetCurrency(userId, await GetCurrency(userId) - currency);
        public async Task<int> SetCurrency(NetUserId userId, int currency)
        {
            await _db.SetServerCurrency(userId, currency);
            return currency;
        }
        public async Task<int> GetCurrency(NetUserId userId) => await _db.GetServerCurrency(userId);

    }
}
