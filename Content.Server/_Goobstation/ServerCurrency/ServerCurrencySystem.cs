using Content.Shared.GameTicking;
using Content.Server._Goobstation.ServerCurrency;
using Content.Shared._Goobstation.ServerCurrency.Events;
using Content.Server.Popups;
using Content.Shared.Popups;

namespace Content.Server._Goobstation.ServerCurrency
{
    /// <summary>
    /// Connects <see cref="ServerCurrencyManager"/> to the simulation state.
    /// </summary>
    public sealed class ServerCurrencySystem : EntitySystem
    {
        [Dependency] private readonly ServerCurrencyManager _currencyMan = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
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

            if(ev.NewAmount > ev.OldAmount)
                _popupSystem.PopupEntity("+" + _currencyMan.Stringify(ev.NewAmount - ev.OldAmount), ev.EntID, ev.EntID, PopupType.Medium);
            else if (ev.NewAmount < ev.OldAmount)
                _popupSystem.PopupEntity("-" + _currencyMan.Stringify(ev.OldAmount - ev.NewAmount), ev.EntID, ev.EntID, PopupType.MediumCaution);
            // I really wanted to do some fancy shit where we also display a little sprite next to the pop-up, but that gets pretty complex for such a simple interaction, so, you get this.
        }
    }
}
