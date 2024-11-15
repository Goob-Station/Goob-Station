using Content.Shared.GameTicking;
using Content.Server._Goobstation.ServerCurrency;
using Content.Shared._Goobstation.ServerCurrency.Events;

namespace Content.Server._Goobstation.ServerCurrency
{
    /// <summary>
    /// Connects <see cref="ServerCurrencyManager"/> to the simulation state.
    /// </summary>
    public sealed class ServerCurrencySystem : EntitySystem
    {
        [Dependency] private readonly ServerCurrencyManager _currencyMan = default!;

        public override void Initialize()
        {
            base.Initialize();
            _currencyMan.BalanceChange += OnPlayerBalanceChange;
            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnd);
        }

        public override void Shutdown()
        {
            base.Shutdown();
            _currencyMan.BalanceChange -= OnPlayerBalanceChange;
        }

        private void OnRoundEnd(RoundRestartCleanupEvent ev)
        {
            _currencyMan.Save();
        }

        /// <summary>
        /// Local event that gets called when a player's balance is updated.
        /// </summary>
        private void OnPlayerBalanceChange(PlayerBalanceChangeEvent ev)
        {
            RaiseLocalEvent(ev.EntID, ref ev);
        }
    }
}
