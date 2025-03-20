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
using Robust.Shared.Network;

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
        // assholeMeter[user] -> (timeSinceAsshole, karmaMult)
        private Dictionary<NetUserId, Tuple<DateTime, float>> assholeMeter = new Dictionary<NetUserId, Tuple<DateTime, float>>();

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
            if (ev.UserSes.AttachedEntity.HasValue)
            {
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
            // Should not lose karma attacking someone you attacked
            // Should lose extra 3x karma if said person is crit and less 0.5x karma if their full health.
            // Should not lose karma if you attack nukie, should lose karma if you unprovoked attack heratic/traitor unless they have killed someone.
            if (_actors.TryGetSession(new EntityUid(ev.Target), out ICommonSession? hitSession))
            {
                if (hitSession != null)
                {
                    float delta = GetMultiplier(session, hitSession, ev.Damage);
                    _karmaMan.RemoveKarma(netUserId, (int) delta);
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
            float dif = 0;
            switch (ev.Grief)
            {
                case PlayerKarmaGriefEvent.GriefType.Explosion:
                    dif = 10 + GetMultiplier(session, 20);
                    break;
                case PlayerKarmaGriefEvent.GriefType.OpenToxicCanister:
                    dif = 10 + GetMultiplier(session, 40);
                    break;
                case PlayerKarmaGriefEvent.GriefType.IgniteOthers:
                    dif = GetMultiplier(session, (int) (8 * ev.Val));
                    break;
                case PlayerKarmaGriefEvent.GriefType.Fire:
                    break;
            }
            if ((int) dif > 0)
            {
                _karmaMan.RemoveKarma(netUserId, (int) dif);
                UpdateAssholeMeter(netUserId, dif);
            }
        }

        // it is important to note that (int) rounds down floats
        private float GetMultiplier(ICommonSession session, int val)
        {
            // TODO: deal with retaliation and allow it to happen without the retaliator losing karma.
            if (_playerSystem.ContentData(session) is not { Mind: { } mindId })
                return 0.5f * val;
            if (_role.MindIsAntagonist(mindId)) // TODO give diffrent antagnists diffrent mults. Nukies should always be 0x except when hitting each other. Traitors shouldn't be plasmaflooding, Nukies can
                return 0;
            if (!_jobs.MindTryGetJob(mindId, out var prototype))
                return 1 * val;
            return prototype.KarmaMult * val;
        }

        private float GetMultiplier(ICommonSession session, ICommonSession targetedSession, int damage)
        {
            float targetMult = GetAssholeMeter(targetedSession.UserId);
            // TODO: 0x if targetedSession is nukie
            return GetMultiplier(session, damage) * targetMult;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="asshole"></param>
        /// <param name="amount">Must be postive</param>.
        private void UpdateAssholeMeter(NetUserId asshole, float amount)
        {
            // 1 > karmaMult > 0: Lower means others lose less karma greifing this player.
            var karmaMult = 1 / (1 + amount * 0.3f);
            var now = DateTime.UtcNow;
            if (assholeMeter.TryGetValue(asshole, out var proofOfAsshole))
            {
                TimeSpan timeSinceLastJerkMoment = now - proofOfAsshole.Item1;
                if (timeSinceLastJerkMoment.TotalMinutes > 10)
                {
                    assholeMeter[asshole] = new Tuple<DateTime, float>(now, proofOfAsshole.Item2 * karmaMult);
                }
            }
            assholeMeter[asshole] = new Tuple<DateTime, float>(now, 1 * karmaMult);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="asshole"></param>
        /// <param name="amount">1 > amount > 0: Lower means others lose less karma greifing this player.</param>.
        private float GetAssholeMeter(NetUserId asshole)
        {
            var now = DateTime.UtcNow;
            if (assholeMeter.TryGetValue(asshole, out var proofOfAsshole))
            {
                TimeSpan timeSinceLastJerkMoment = now - proofOfAsshole.Item1;
                if (timeSinceLastJerkMoment.TotalMinutes > 10)
                {
                    return proofOfAsshole.Item2;
                }
            }
            return 1;
        }
    }
}
