using Content.Shared.GameTicking;
using Content.Shared._Goobstation.ServerCurrency.Events;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Server.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Roles.Jobs;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared._Goobstation.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
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
        [Dependency] private readonly SharedJobSystem _jobs = default!;
        [Dependency] private readonly IPlayerManager _players = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        private int _goobcoinsPerPlayer = 10;
        private int _goobcoinsNonAntagMultiplier = 3;
        private int _goobcoinsServerMultiplier = 1;
        private int _goobcoinsMinPlayers;

        public override void Initialize()
        {
            base.Initialize();
            _currencyMan.BalanceChange += OnBalanceChange;
            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEndCleanup);
            SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
            SubscribeNetworkEvent<PlayerBalanceRequestEvent>(OnBalanceRequest);
            Subs.CVar(_cfg, GoobCVars.GoobcoinsPerPlayer, value => _goobcoinsPerPlayer = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinNonAntagMultiplier, value => _goobcoinsNonAntagMultiplier = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinServerMultiplier, value => _goobcoinsServerMultiplier = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinMinPlayers, value => _goobcoinsMinPlayers = value, true);
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
            if (_players.PlayerCount < _goobcoinsMinPlayers)
                return;

            var query = EntityQueryEnumerator<MindContainerComponent>();

            while (query.MoveNext(out var uid, out var mindContainer))
            {
                var isBorg = HasComp<BorgChassisComponent>(uid);
                if (!(HasComp<HumanoidAppearanceComponent>(uid)
                    || HasComp<BorgBrainComponent>(uid)
                    || isBorg))
                    continue;

                if (mindContainer.Mind.HasValue)
                {
                    var mind = Comp<MindComponent>(mindContainer.Mind.Value);
                    if (mind is not null
                        && (isBorg || !_mind.IsCharacterDeadIc(mind)) // Borgs count always as dead so I'll just throw them a bone and give them an exception.
                        && mind.OriginalOwnerUserId.HasValue)
                    {
                        int money = _goobcoinsPerPlayer;
                        var session = mind.Session;
                        if (session is not null)
                        {
                            money += _jobs.GetJobGoobcoins(session);
                            if (!_jobs.CanBeAntag(session))
                                money *= _goobcoinsNonAntagMultiplier;
                        }

                        if (_goobcoinsServerMultiplier != 1)
                            money *= _goobcoinsServerMultiplier;

                        _currencyMan.AddCurrency(mind.OriginalOwnerUserId.Value, money);
                    }
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
