// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Milon <milonpl.git@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tim <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.ServerCurrency;
using Content.Server._RMC14.LinkAccount;
using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.Roles.Jobs;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Server.Player;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Server.ServerCurrency
{
    /// <summary>
    /// Connects <see cref="ServerCurrencyManager"/> to the simulation state.
    /// </summary>
    public sealed class ServerCurrencySystem : EntitySystem
    {
        [Dependency] private readonly ICommonCurrencyManager _currencyMan = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedMindSystem _mind = default!;
        [Dependency] private readonly SharedJobSystem _jobs = default!;
        [Dependency] private readonly IPlayerManager _players = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly LinkAccountManager _linkAccount = default!;
        [Dependency] private readonly GameTicker _gameTicker = default!;

        private int _goobcoinsPerPlayer = 10;
        private int _goobcoinsNonAntagMultiplier = 1;
        private int _goobcoinsServerMultiplier = 1;
        private int _goobcoinsMinPlayers;
        private bool _goobcoinsUseLowPopMultiplier;
        private double _goobcoinsLowPopMultiplierStrength = 1.0;
        private bool _goobcoinsUseShortRoundPenalty = true;
        private int _goobcoinsShortRoundPenaltyTargetMinutes = 50;

        public override void Initialize()
        {
            base.Initialize();
            _currencyMan.BalanceChange += OnBalanceChange;
            SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
            SubscribeNetworkEvent<PlayerBalanceRequestEvent>(OnBalanceRequest);
            Subs.CVar(_cfg, GoobCVars.GoobcoinsPerPlayer, value => _goobcoinsPerPlayer = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinNonAntagMultiplier, value => _goobcoinsNonAntagMultiplier = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinServerMultiplier, value => _goobcoinsServerMultiplier = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinMinPlayers, value => _goobcoinsMinPlayers = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinUseLowpopMultiplier, value => _goobcoinsUseLowPopMultiplier = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinLowpopMultiplierStrength, value => _goobcoinsLowPopMultiplierStrength = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinUseShortRoundPenalty, value => _goobcoinsUseShortRoundPenalty = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinShortRoundPenaltyTargetMinutes, value => _goobcoinsShortRoundPenaltyTargetMinutes = value, true);
        }

        public override void Shutdown()
        {
            base.Shutdown();
            _currencyMan.BalanceChange -= OnBalanceChange;
        }

        private void OnRoundEndText(RoundEndTextAppendEvent ev)
        {
            if (_players.PlayerCount < _goobcoinsMinPlayers)
                return;

            var lowPopMultiplier = 1.0 - (_players.PlayerCount / (double)_players.MaxPlayers);

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
                        && mind.OriginalOwnerUserId.HasValue
                        && _players.TryGetSessionById(mind.UserId, out var session))
                    {
                        int money = _goobcoinsPerPlayer;
                        if (session is not null)
                        {
                            money += _jobs.GetJobGoobcoins(session);
                            if (!_jobs.CanBeAntag(session))
                                money *= _goobcoinsNonAntagMultiplier;
                        }

                        if(_goobcoinsUseLowPopMultiplier)
                            money += (int)Math.Round(money * lowPopMultiplier * _goobcoinsLowPopMultiplierStrength);

                        if (_goobcoinsServerMultiplier != 1)
                            money *= _goobcoinsServerMultiplier;

                        if (session != null && _linkAccount.GetPatron(session)?.Tier != null)
                            money *= 2;

                        if (_goobcoinsUseShortRoundPenalty)
                        {
                            var roundMinutesActual = _gameTicker.RoundDuration().TotalMinutes;
                            money = (int) (money * Math.Min(1, roundMinutesActual / _goobcoinsShortRoundPenaltyTargetMinutes));
                        }

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

            if (ev.UserSes.AttachedEntity.HasValue)
            {
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
