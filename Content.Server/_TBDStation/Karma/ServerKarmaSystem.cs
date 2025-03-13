using Content.Shared.GameTicking;
using Content.Shared._TBDStation.ServerKarma.Events;
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
using Robust.Shared.Player;
using Content.Server.Roles;
using Content.Shared.Players;
using Content.Shared.Roles;

namespace Content.Server._TBDStation.ServerKarma
{
    /// <summary>
    /// Connects <see cref="ServerKarmaManager"/> to the simulation state.
    /// </summary>
    public sealed class ServerKarmaSystem : EntitySystem
    {
        [Dependency] private readonly ServerKarmaManager _karmaMan = default!;
        [Dependency] private readonly ActorSystem _actors = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedMindSystem _mind = default!;
        [Dependency] private readonly SharedJobSystem _jobs = default!;
        [Dependency] private readonly RoleSystem _role = default!;
        [Dependency] private readonly IPlayerManager _players = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly SharedPlayerSystem _playerSystem = default!;

        private int _goobcoinsPerPlayer = 10;
        private int _goobcoinsNonAntagMultiplier = 3;
        private int _goobcoinsServerMultiplier = 1;
        private int _goobcoinsMinPlayers;

        public override void Initialize()
        {
            base.Initialize();
            _karmaMan.KarmaChange += OnKarmaChange;
            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEndCleanup);
            SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
            SubscribeNetworkEvent<PlayerKarmaRequestEvent>(OnKarmaRequest);
            SubscribeLocalEvent<PlayerKarmaHitEvent>(OnKarmaHit);
            SubscribeLocalEvent<PlayerKarmaGriefEvent>(OnKarmaGrief);
            Subs.CVar(_cfg, GoobCVars.GoobcoinsPerPlayer, value => _goobcoinsPerPlayer = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinNonAntagMultiplier, value => _goobcoinsNonAntagMultiplier = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinServerMultiplier, value => _goobcoinsServerMultiplier = value, true);
            Subs.CVar(_cfg, GoobCVars.GoobcoinMinPlayers, value => _goobcoinsMinPlayers = value, true);
        }
        public override void Shutdown()
        {
            base.Shutdown();
            _karmaMan.KarmaChange -= OnKarmaChange;
        }

        private void OnRoundEndCleanup(RoundRestartCleanupEvent ev)
        {
            _karmaMan.Save();
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
                        _karmaMan.AddKarma(mind.OriginalOwnerUserId.Value, 50);
                    }
                }
            }
        }

        private void OnKarmaRequest(PlayerKarmaRequestEvent ev, EntitySessionEventArgs eventArgs)
        {
            var senderSession = eventArgs.SenderSession;
            var karma = _karmaMan.GetKarma(senderSession.UserId);
            RaiseNetworkEvent(new PlayerKarmaUpdateEvent(karma, karma), senderSession);
        }

        /// <summary>
        /// Calls event that when a player's karma is updated.
        /// Also handles popups
        /// </summary>
        private void OnKarmaChange(PlayerKarmaChangeEvent ev)
        {
            RaiseNetworkEvent(new PlayerKarmaUpdateEvent(ev.NewKarma, ev.OldKarma), ev.UserSes);
            if(ev.UserSes.AttachedEntity.HasValue){
                var userEnt = ev.UserSes.AttachedEntity.Value;
                if (ev.NewKarma > ev.OldKarma)
                    _popupSystem.PopupEntity("+" + _karmaMan.Stringify(ev.NewKarma - ev.OldKarma), userEnt, userEnt, PopupType.Medium);
                else if (ev.NewKarma < ev.OldKarma)
                    _popupSystem.PopupEntity("-" + _karmaMan.Stringify(ev.OldKarma - ev.NewKarma), userEnt, userEnt, PopupType.MediumCaution);
                // I really wanted to do some fancy shit where we also display a little sprite next to the pop-up, but that gets pretty complex for such a simple interaction, so, you get this.
            }
        }

        private void OnKarmaHit(PlayerKarmaHitEvent ev)
        {
            if (!_actors.TryGetSession(new EntityUid(ev.User), out ICommonSession? session))
                return;
            if (session == null)
                return;
            var netUserId = session.UserId;
            if (_actors.TryGetSession(new EntityUid(ev.Target), out ICommonSession? hitSession))
            {
                if (hitSession != null)
                {
                    int delta = GetMultiplier(session, ev.Damage);
                    _karmaMan.RemoveKarma(netUserId, delta);
                }
            }
        }

        private void OnKarmaGrief(PlayerKarmaGriefEvent ev)
        {
            if (!_actors.TryGetSession(new EntityUid(ev.User), out ICommonSession? session))
                return;
            if (session == null)
                return;
            var netUserId = session.UserId;
            switch (ev.Grief)
            {
                case PlayerKarmaGriefEvent.GriefType.Explosion:
                    _karmaMan.RemoveKarma(netUserId, GetMultiplier(session, 20));
                    break;
                case PlayerKarmaGriefEvent.GriefType.Fire:
                    break;
            }
        }

        private int GetMultiplier(ICommonSession session, int val)
        {
            // TODO: deal with retaliation and allow it to happen without the retaliator losing karma.
            if (_playerSystem.ContentData(session) is not { Mind: { } mindId })
                return (int) (0.5f * val);
            if (_role.MindIsAntagonist(mindId))
                return 0;
            if (!_jobs.MindTryGetJob(mindId, out var prototype))
                return 1 * val;
            return (int) (prototype.KarmaMult * val);
        }
    }
}
