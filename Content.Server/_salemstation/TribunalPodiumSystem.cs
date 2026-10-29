using System.Linq;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Shared.Access.Systems;
using Content.Shared.Buckle.Components;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared._salemstation;
using Content.Server.Mind;
using Content.Server.Station.Systems;
using Content.Shared.Actions;
using Content.Shared.Ghost;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._salemstation
{
    public sealed partial class TribunalPodiumSystem : EntitySystem
    {
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly AccessReaderSystem _access = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!;
        [Dependency] private readonly MindSystem _minds = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly MobStateSystem _mobState = default!;
        [Dependency] private readonly StationSystem _station = default!;

        // Push a fresh UI state (vote counts + countdown) to viewers once per second.
        private static readonly TimeSpan UiUpdateInterval = TimeSpan.FromSeconds(1);
        private TimeSpan _nextUiUpdate;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TribunalPodiumComponent, InteractHandEvent>(OnInteract);
            SubscribeLocalEvent<MindContainerComponent, MindAddedMessage>(OnMindAdded);
            SubscribeLocalEvent<MindGotAddedEvent>(OnMindGotAdded);
            SubscribeLocalEvent<AfterAntagEntitySelectedEvent>(OnJesterSelected);
            SubscribeLocalEvent<JesterGhostKillActionEvent>(OnJesterGhostKill);
            SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
            InitializeBui();
        }

        private void OnMindAdded(EntityUid uid, MindContainerComponent component, MindAddedMessage args)
        {
            TryGrantJesterGhostAction(uid, args.Mind.Comp);
        }

        private void OnMindGotAdded(MindGotAddedEvent args)
        {
            if (args.Mind.Comp.OwnedEntity is not { } entity)
                return;

            TryGrantJesterGhostAction(entity, args.Mind.Comp);
        }

        private void TryGrantJesterGhostAction(EntityUid entity, MindComponent mind)
        {
            if (!HasComp<GhostComponent>(entity))
                return;

            foreach (var role in mind.MindRoles)
            {
                if (!TryComp<JesterRoleComponent>(role, out var jester) || !jester.ExecutedByTribunal || jester.GhostKillUsed || jester.GhostKillAction != null)
                    continue;

                _actions.AddAction(entity, ref jester.GhostKillAction, "ActionJesterGhostKill");
                Dirty(role, jester);
                break;
            }
        }

        private void OnJesterSelected(ref AfterAntagEntitySelectedEvent args)
        {
            if (args.Session is null || !TryComp<MindContainerComponent>(args.EntityUid, out var mindContainer) || mindContainer.Mind is not { } mindId ||
                !TryComp<MindComponent>(mindId, out var mind))
                return;

            foreach (var role in mind.MindRoles)
            {
                if (!HasComp<JesterRoleComponent>(role))
                    continue;

                _chatManager.DispatchServerMessage(args.Session,
                    "You are the Jester. Your objective is to be found guilty and executed by the Tribunal, then use your ghost ability to kill one living player on the station.");
                break;
            }
        }

        private void OnInteract(EntityUid uid, TribunalPodiumComponent component, InteractHandEvent args)
        {
            if (args.Handled || component.CurrentState != TrialState.Idle)
                return;

            // Only someone carrying Captain-level access may convene a trial.
            if (!_access.FindAccessTags(args.User).Contains("Captain"))
            {
                _popup.PopupEntity("You need the Captain's authority to convene a tribunal!", uid, args.User);
                return;
            }

            if (!TryComp<StrapComponent>(uid, out var strap) || strap.BuckledEntities.Count == 0)
            {
                _popup.PopupEntity("There is no prisoner secured to the podium!", uid, args.User);
                return;
            }

            component.Prisoner = strap.BuckledEntities.First();
            component.Votes.Clear();
            StartDiscussionPhase(uid, component);
            args.Handled = true;
        }

        private void StartDiscussionPhase(EntityUid uid, TribunalPodiumComponent component)
        {
            component.CurrentState = TrialState.Discussion;
            component.StateEndTime = _timing.CurTime + component.DiscussionDuration;

            _chat.DispatchGlobalAnnouncement(
                $"A formal Tribunal has been called by the Captain! {Name(component.Prisoner!.Value)} stands accused. The {(int) component.DiscussionDuration.TotalMinutes}-minute evidence and discussion period begins now.",
                "Tribunal AI",
                colorOverride: Color.DeepSkyBlue);

            UpdateUserInterface(uid, component);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            var curTime = _timing.CurTime;

            var pushUiUpdate = curTime >= _nextUiUpdate;
            if (pushUiUpdate)
                _nextUiUpdate = curTime + UiUpdateInterval;

            var query = EntityQueryEnumerator<TribunalPodiumComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                if (comp.CurrentState == TrialState.Idle)
                    continue;

                if (curTime >= comp.StateEndTime)
                    AdvanceTrialState(uid, comp);
                else if (pushUiUpdate)
                    UpdateUserInterface(uid, comp);
            }
        }

        private void AdvanceTrialState(EntityUid uid, TribunalPodiumComponent comp)
        {
            switch (comp.CurrentState)
            {
                case TrialState.Discussion:
                    comp.CurrentState = TrialState.Voting;
                    comp.StateEndTime = _timing.CurTime + comp.VotingDuration;
                    comp.Votes.Clear();
                    _chat.DispatchGlobalAnnouncement(
                        $"Discussion time is over. Crewmates have {(int) comp.VotingDuration.TotalSeconds} seconds to cast their votes at the podium!",
                        "Tribunal AI");
                    break;

                case TrialState.Voting:
                    CountVotes(comp, out var guilty, out var innocent);

                    if (guilty > innocent)
                    {
                        comp.CurrentState = TrialState.Execution;
                        comp.StateEndTime = _timing.CurTime + comp.LastWordsDuration;
                        _chat.DispatchGlobalAnnouncement(
                            $"The verdict is GUILTY ({guilty} to {innocent}). The prisoner has {(int) comp.LastWordsDuration.TotalSeconds} seconds for last words.",
                            "Tribunal AI",
                            colorOverride: Color.Red);
                    }
                    else
                    {
                        _chat.DispatchGlobalAnnouncement(
                            $"The verdict is INNOCENT ({innocent} to {guilty}). The accused walks free.",
                            "Tribunal AI",
                            colorOverride: Color.LimeGreen);
                        EndTrial(comp);
                    }
                    break;

                case TrialState.Execution:
                    ExecutePrisoner(comp.Prisoner);
                    EndTrial(comp);
                    break;
            }

            UpdateUserInterface(uid, comp);
        }

        private static void EndTrial(TribunalPodiumComponent comp)
        {
            comp.CurrentState = TrialState.Idle;
            comp.Prisoner = null;
            comp.Votes.Clear();
        }

        private static void CountVotes(TribunalPodiumComponent comp, out int guilty, out int innocent)
        {
            guilty = 0;
            innocent = 0;

            foreach (var vote in comp.Votes.Values)
            {
                if (vote)
                    guilty++;
                else
                    innocent++;
            }
        }

        private void ExecutePrisoner(EntityUid? prisoner)
        {
            if (prisoner == null || !Exists(prisoner))
                return;

            if (_minds.TryGetMind(prisoner.Value, out _, out var mind))
            {
                foreach (var role in mind.MindRoles)
                {
                    if (!TryComp<JesterRoleComponent>(role, out var jester))
                        continue;

                    jester.ExecutedByTribunal = true;
                    Dirty(role, jester);
                    break;
                }
            }

            // SS14 uses specific string IDs for damage types. "Asphyxiation" is standard.
            var damageSpec = new DamageSpecifier();
            damageSpec.DamageDict.Add("Asphyxiation", 200);

            // 'ignoreResistances: true' ensures armor or internals don't accidentally block the execution.
            _damageable.TryChangeDamage(prisoner.Value, damageSpec, ignoreResistances: true);

            _chat.DispatchGlobalAnnouncement(
                $"{Name(prisoner.Value)} has been suffocated by order of the Tribunal.",
                "Tribunal AI",
                colorOverride: Color.Tomato);
        }

        private void OnJesterGhostKill(JesterGhostKillActionEvent args)
        {
            if (args.Handled || !HasComp<GhostComponent>(args.Performer) ||
                !TryComp<ActorComponent>(args.Target, out var actor) || actor.PlayerSession == null ||
                !TryComp<MobStateComponent>(args.Target, out var mobState) || _mobState.IsDead(args.Target, mobState) ||
                _station.GetOwningStation(args.Target) == null ||
                !_minds.TryGetMind(args.Performer, out _, out var mind))
                return;

            JesterRoleComponent? jester = null;
            EntityUid jesterRole = default;
            foreach (var role in mind.MindRoles)
            {
                if (!TryComp<JesterRoleComponent>(role, out var roleComp) || roleComp.GhostKillAction != args.Action.Owner)
                    continue;

                jester = roleComp;
                jesterRole = role;
                break;
            }

            if (jester == null || !jester.ExecutedByTribunal)
                return;

            _mobState.ChangeMobState(args.Target, MobState.Dead, mobState, args.Performer);
            _actions.RemoveAction(args.Performer, jester.GhostKillAction);
            jester.GhostKillAction = null;
            jester.GhostKillUsed = true;
            Dirty(jesterRole, jester);
            args.Handled = true;
        }

        private void OnRoundEndTextAppend(RoundEndTextAppendEvent args)
        {
            var query = EntityQueryEnumerator<MindComponent>();
            while (query.MoveNext(out _, out var mind))
            {
                foreach (var role in mind.MindRoles)
                {
                    if (!TryComp<JesterRoleComponent>(role, out var jester) || !jester.ExecutedByTribunal)
                        continue;

                    args.AddLine($"{mind.CharacterName} was a Jester and was successfully executed by the Tribunal.");
                    break;
                }
            }
        }
    }
}
