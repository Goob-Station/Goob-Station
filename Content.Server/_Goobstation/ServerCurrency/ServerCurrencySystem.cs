using Content.Shared.GameTicking;
using Content.Shared._Goobstation.ServerCurrency.Events;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Server.GameTicking;
using Content.Shared.Humanoid;

namespace Content.Server._Goobstation.ServerCurrency
{
    /// <summary>
    /// Connects <see cref="ServerCurrencyManager"/> to the simulation state.
    /// </summary>
    public sealed class ServerCurrencySystem : EntitySystem
    {
        [Dependency] private readonly ServerCurrencyManager _currencyMan = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedMindSystem _mind = default!;
        public override void Initialize()
        {
            base.Initialize();
            _currencyMan.BalanceChange += OnBalanceChange;
            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEndCleanup);
            SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
            SubscribeNetworkEvent<PlayerBalanceRequestEvent>(OnBalanceRequest);
        }

        public override void Shutdown()
        {
            base.Shutdown();
            _currencyMan.BalanceChange -= OnBalanceChange;
        }

        private void OnRoundEndCleanup(RoundRestartCleanupEvent ev)
        {
            _currencyMan.Save();
        }

        private void OnRoundEndText(RoundEndTextAppendEvent ev)
        {
            var query = EntityQueryEnumerator<MindContainerComponent, HumanoidAppearanceComponent>();

            while (query.MoveNext(out _, out var mindContainer, out _))
            {
                if (mindContainer.Mind.HasValue)
                {
                    var mind = Comp<MindComponent>(mindContainer.Mind.Value);
                    if (mind is not null && !_mind.IsCharacterDeadIc(mind))
                        if (mind.OriginalOwnerUserId.HasValue)
                            _currencyMan.AddCurrency(mind.OriginalOwnerUserId.Value, 10);
                }
            }
        }

        private void OnBalanceRequest(PlayerBalanceRequestEvent ev, EntitySessionEventArgs eventArgs)
        {
            var senderSession = eventArgs.SenderSession;
            var balance = _currencyMan.GetBalance(senderSession.UserId);
            RaiseNetworkEvent(new PlayerBalanceUpdateEvent(balance, balance), senderSession);

        }

        /// <summary>
        /// Calls event that when a player's balance is updated.
        /// Also handles popups
        /// </summary>
        private void OnBalanceChange(PlayerBalanceChangeEvent ev)
        {
            RaiseNetworkEvent(new PlayerBalanceUpdateEvent(ev.NewBalance, ev.OldBalance), ev.UserSes);


            if(ev.UserSes.AttachedEntity.HasValue){
                var userEnt = ev.UserSes.AttachedEntity.Value;
                if (ev.NewBalance > ev.OldBalance)
                    _popupSystem.PopupEntity("+" + _currencyMan.Stringify(ev.NewBalance - ev.OldBalance), userEnt, userEnt, PopupType.Medium);
                else if (ev.NewBalance < ev.OldBalance)
                    _popupSystem.PopupEntity("-" + _currencyMan.Stringify(ev.OldBalance - ev.NewBalance), userEnt, userEnt, PopupType.MediumCaution);
                // I really wanted to do some fancy shit where we also display a little sprite next to the pop-up, but that gets pretty complex for such a simple interaction, so, you get this.
            }
        }
    }
}
