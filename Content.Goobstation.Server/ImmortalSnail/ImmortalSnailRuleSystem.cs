using Content.Goobstation.Shared.ImmortalSnail;
using Content.Goobstation.Server.ImmortalSnail.Objectives;
using Content.Server.Chat.Systems;
using Content.Server.EUI;
using Content.Server.GameTicking.Rules;
using Content.Server.Station.Systems;
using Content.Server.Antag;
using Content.Server.Roles;
using Content.Server.Ghost.Roles.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Robust.Shared.Player;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Goobstation.Server.ImmortalSnail;

public sealed class ImmortalSnailRuleSystem : GameRuleSystem<ImmortalSnailRuleComponent>
{
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedGodmodeSystem _godmode = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ImmortalSnailObjectiveSystem _objectives = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ImmortalSnailSystem _snailSystem = default!;

    private readonly List<ICommonSession> _triedPlayers = new();
    private readonly Dictionary<EntityUid, AcceptImmortalSnailEui> _openEuis = new();
    private readonly Dictionary<EntityUid, EntityUid> _spawnerToTarget = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ImmortalSnailRuleComponent, AfterAntagEntitySelectedEvent>(OnAfterAntagSelected);
        SubscribeLocalEvent<ImmortalSnailComponent, MobStateChangedEvent>(OnSnailDeath);
        SubscribeLocalEvent<ImmortalSnailComponent, EntityTerminatingEvent>(OnSnailTerminating);
        SubscribeLocalEvent<ImmortalSnailComponent, GhostRoleSpawnerUsedEvent>(OnSnailSpawned);
        SubscribeLocalEvent<ImmortalSnailTargetRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<ImmortalSnailSmitedComponent, GetBriefingEvent>(OnGetSmitedBriefing);
        SubscribeLocalEvent<ImmortalSnailTargetComponent, MobStateChangedEvent>(OnTargetDeath);
        SubscribeLocalEvent<ImmortalSnailTargetComponent, EntityTerminatingEvent>(OnTargetTerminating);
    }

    private void OnGetBriefing(Entity<ImmortalSnailTargetRoleComponent> role, ref GetBriefingEvent args)
    {
        if (HasComp<ImmortalSnailSmitedComponent>(role.Owner))
            return;

        args.Append(Loc.GetString("immortal-snail-target-role-greeting"));
    }

    private void OnGetSmitedBriefing(Entity<ImmortalSnailSmitedComponent> role, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("immortal-snail-smited-role-greeting"));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var euiList = _openEuis.ToList();
        foreach (var (ruleEntity, eui) in euiList)
            if (eui.HasTimedOut())
            {
                _openEuis.Remove(ruleEntity);
                OnPlayerDeclineOffer(ruleEntity);
                eui.Close();
            }
    }

    protected override void Started(EntityUid uid, ImmortalSnailRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        _triedPlayers.Clear();
        _openEuis.Clear();
        _spawnerToTarget.Clear();
    }

    private void OnAfterAntagSelected(Entity<ImmortalSnailRuleComponent> rule, ref AfterAntagEntitySelectedEvent args)
    {
        var snail = args.EntityUid;

        if (!TryComp<ImmortalSnailComponent>(snail, out var snailComp))
            return;

        rule.Comp.SnailEntity = snail;
        snailComp.RuleEntity = rule.Owner;
        Dirty(snail, snailComp);

        SelectRandomPlayer(rule, rule.Comp);
    }

    /// <summary>
    /// Only used for the admin smite. Tracks the spawned snail so it can be deleted later.
    /// </summary>
    private void OnSnailSpawned(EntityUid snail, ImmortalSnailComponent component, GhostRoleSpawnerUsedEvent args)
    {
        if (_spawnerToTarget.TryGetValue(args.Spawner, out var target))
        {
            component.Target = target;
            Dirty(snail, component);

            if (_mind.TryGetMind(target, out var targetMind, out var mindComp))
                foreach (var roleId in mindComp.MindRoles)
                    if (TryComp<ImmortalSnailSmitedComponent>(roleId, out var smitedComp))
                    {
                        smitedComp.SnailEntity = snail;
                        smitedComp.SnailSpawner = null;
                        Dirty(roleId, smitedComp);
                        break;
                    }

            var targetSetEvent = new ImmortalSnailTargetSetEvent();
            RaiseLocalEvent(snail, ref targetSetEvent);
        }
    }

    private void SelectRandomPlayer(EntityUid ruleEntity, ImmortalSnailRuleComponent component)
    {
        var candidates = new List<(EntityUid entity, ICommonSession session)>();
        var allPotentialCandidates = new List<(EntityUid entity, ICommonSession session)>();

        var query = EntityQueryEnumerator<HumanoidAppearanceComponent, MobStateComponent, ActorComponent>();
        while (query.MoveNext(out var uid, out _, out var mobState, out var actor))
        {
            var hasTargetComponent = HasComp<ImmortalSnailTargetComponent>(uid);
            var isAlive = !_mobState.IsDead(uid, mobState);
            var hasStation = _station.GetOwningStation(uid) != null;
            var isEligible = !hasTargetComponent && isAlive && hasStation;

            // Tried players are only excluded from normal selection, not forced selection to prevent them from being asked multiple times.
            if (!_triedPlayers.Contains(actor.PlayerSession) && isEligible)
                candidates.Add((uid, actor.PlayerSession));

            if (isEligible)
                allPotentialCandidates.Add((uid, actor.PlayerSession));
        }

        // If no untried candidates but we have eligible players, force select someone
        if (candidates.Count == 0 && allPotentialCandidates.Count > 0)
        {
            var (selectedEntity, selectedSession) = _random.Pick(allPotentialCandidates);
            component.TargetEntity = selectedEntity;

            OnPlayerAcceptOffer(ruleEntity, selectedSession, playNormalAnnouncement: false);

            if (component.PlayAnnouncements)
            {
                var targetName = MetaData(selectedEntity).EntityName;
                _chat.DispatchGlobalAnnouncement(
                    Loc.GetString("immortal-snail-forced-announcement", ("target", targetName)),
                    sender: Loc.GetString("immortal-snail-announcement-sender"),
                    announcementSound: component.AnnouncementSound,
                    colorOverride: Color.Yellow);
            }

            return;
        }

        if (candidates.Count == 0)
            return;

        var (normalSelectedEntity, normalSelectedSession) = _random.Pick(candidates);
        component.TargetEntity = normalSelectedEntity;

        _triedPlayers.Add(normalSelectedSession);

        var eui = new AcceptImmortalSnailEui(ruleEntity, normalSelectedSession, this);
        _euiManager.OpenEui(eui, normalSelectedSession);
        _openEuis[ruleEntity] = eui;
    }

    /// <summary>
    /// Spawns an immortal snail spawner that targets the specified entity without giving them immortality.
    /// Used for admin smites.
    /// </summary>
    public void SpawnSnailSmite(EntityUid target)
    {
        if (!Exists(target))
            return;

        var xform = Transform(target);

        var spawner = Spawn("SpawnPointGhostImmortalSnailSmite", xform.Coordinates);

        // Add marker component to track target deletion
        EnsureComp<ImmortalSnailTargetComponent>(target);

        if (_mind.TryGetMind(target, out var targetMind, out var mindComp))
        {
            _objectives.AddTargetObjectives(targetMind);

            _role.MindAddRole(targetMind, "MindRoleImmortalSnailTarget", mindComp, silent: false);

            if (_role.MindHasRole<ImmortalSnailTargetRoleComponent>((targetMind, mindComp), out var roleEntity))
            {
                var smitedComp = EnsureComp<ImmortalSnailSmitedComponent>(roleEntity.Value.Owner);
                smitedComp.SnailSpawner = spawner;
                smitedComp.SnailEntity = null;
                Dirty(roleEntity.Value.Owner, smitedComp);
            }

            _antag.SendBriefing(target,
                Loc.GetString("immortal-snail-smited-role-greeting"),
                Color.Red,
                null);
        }

        _spawnerToTarget[spawner] = target;
    }

    public void OnPlayerAcceptOffer(EntityUid ruleEntity, ICommonSession session, bool playNormalAnnouncement = true)
    {
        if (!TryComp<ImmortalSnailRuleComponent>(ruleEntity, out var ruleComponent))
            return;

        _openEuis.Remove(ruleEntity);

        var targetEntity = ruleComponent.TargetEntity;
        var snailEntity = ruleComponent.SnailEntity;

        if (!TryComp<ImmortalSnailComponent>(snailEntity, out var snailComponent))
            return;

        snailComponent.Target = targetEntity;
        Dirty(snailEntity.Value, snailComponent);

        if (targetEntity != null)
        {
            EnsureComp<ImmortalSnailTargetComponent>(targetEntity.Value);

            // Only grant godmode to non-smited targets
            var isSmited = false;
            if (_mind.TryGetMind(targetEntity.Value, out var checkMind, out var checkMindComp))
                foreach (var roleId in checkMindComp.MindRoles)
                    if (HasComp<ImmortalSnailSmitedComponent>(roleId))
                    {
                        isSmited = true;
                        break;
                    }

            if (!isSmited)
                _godmode.EnableGodmode(targetEntity.Value);

            if (_mind.TryGetMind(targetEntity.Value, out var targetMind, out var mindComp))
            {
                _objectives.AddTargetObjectives(targetMind);

                _role.MindAddRole(targetMind, "MindRoleImmortalSnailTarget", mindComp, silent: false);

                var greeting = isSmited
                    ? "immortal-snail-smited-role-greeting"
                    : "immortal-snail-target-role-greeting";

                _antag.SendBriefing(targetEntity.Value,
                    Loc.GetString(greeting),
                    isSmited ? Color.Red : Color.Yellow,
                    isSmited ? null : ruleComponent.HonkSound);
            }
        }

        var targetSetEvent = new ImmortalSnailTargetSetEvent();
        RaiseLocalEvent(snailEntity.Value, ref targetSetEvent);

        if (playNormalAnnouncement && ruleComponent.PlayAnnouncements)
            _chat.DispatchGlobalAnnouncement(
                Loc.GetString("immortal-snail-announcement"),
                sender: Loc.GetString("immortal-snail-announcement-sender"),
                announcementSound: ruleComponent.AnnouncementSound,
                colorOverride: Color.Yellow);
    }

    public void OnPlayerDeclineOffer(EntityUid ruleEntity)
    {
        if (!TryComp<ImmortalSnailRuleComponent>(ruleEntity, out var component))
            return;

        _openEuis.Remove(ruleEntity);

        SelectRandomPlayer(ruleEntity, component);
    }

    private void OnSnailDeath(EntityUid uid, ImmortalSnailComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        HandleSnailRemoval(uid, component);
    }

    private void OnSnailTerminating(EntityUid uid, ImmortalSnailComponent component, ref EntityTerminatingEvent args)
    {
        HandleSnailRemoval(uid, component);
    }

    private void HandleSnailRemoval(EntityUid snailUid, ImmortalSnailComponent component)
    {
        if (component.Target != null && Exists(component.Target.Value))
        {
            var target = component.Target.Value;

            if (HasComp<GodmodeComponent>(target))
                _godmode.DisableGodmode(target);

            if (HasComp<ImmortalSnailTargetComponent>(target))
                RemCompDeferred<ImmortalSnailTargetComponent>(target);

            if (_mind.TryGetMind(target, out var targetMind, out var mindComp))
                foreach (var roleId in mindComp.MindRoles)
                    if (HasComp<ImmortalSnailSmitedComponent>(roleId))
                        RemCompDeferred<ImmortalSnailSmitedComponent>(roleId);

            // Check if announcements should be played
            if (component.RuleEntity != null
                && TryComp<ImmortalSnailRuleComponent>(component.RuleEntity.Value, out var ruleComp)
                && ruleComp.PlayAnnouncements)
                _chat.DispatchGlobalAnnouncement(
                    Loc.GetString("immortal-snail-death-announcement"),
                    sender: Loc.GetString("immortal-snail-announcement-sender"),
                    announcementSound: ruleComp.HonkSound,
                    colorOverride: Color.Yellow);
        }

        component.Target = null;
        Dirty(snailUid, component);
    }

    private void OnTargetDeath(EntityUid uid, ImmortalSnailTargetComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        HandleTargetRemoval(uid);
    }

    private void OnTargetTerminating(EntityUid uid, ImmortalSnailTargetComponent component, ref EntityTerminatingEvent args)
    {
        HandleTargetRemoval(uid);
    }

    private void HandleTargetRemoval(EntityUid uid)
    {
        if (!Exists(uid))
            return;

        var targetName = MetaData(uid).EntityName;

        if (HasComp<ImmortalSnailTargetComponent>(uid))
            RemCompDeferred<ImmortalSnailTargetComponent>(uid);

        if (_mind.TryGetMind(uid, out var targetMind, out var mindComp))
            foreach (var roleId in mindComp.MindRoles)
                if (TryComp<ImmortalSnailSmitedComponent>(roleId, out var smitedComp))
                {
                    if (smitedComp.SnailEntity != null && Exists(smitedComp.SnailEntity.Value))
                        QueueDel(smitedComp.SnailEntity.Value);

                    if (smitedComp.SnailSpawner != null && Exists(smitedComp.SnailSpawner.Value))
                    {
                        _spawnerToTarget.Remove(smitedComp.SnailSpawner.Value);
                        QueueDel(smitedComp.SnailSpawner.Value);
                    }

                    return;
                }

        var query = EntityQueryEnumerator<ImmortalSnailComponent>();
        while (query.MoveNext(out var snailUid, out var snailComp))
        {
            if (snailComp.Target != uid)
                continue;

            if (!Exists(snailUid))
            {
                snailComp.Target = null;
                continue;
            }

            var shouldGiveBuffs = !HasComp<Content.Shared._Goobstation.Wizard.WizardComponent>(snailUid);

            if (shouldGiveBuffs)
            {
                ImmortalSnailRuleComponent? ruleComp = null;
                if (snailComp.RuleEntity != null)
                    TryComp<ImmortalSnailRuleComponent>(snailComp.RuleEntity.Value, out ruleComp);

                _snailSystem.GiveSnailBuffs(snailUid, targetName, ruleComp, playAnnouncement: false);

                if (ruleComp != null && ruleComp.PlayAnnouncements)
                    _chat.DispatchGlobalAnnouncement(
                        Loc.GetString("immortal-snail-target-died-announcement", ("target", targetName)),
                        sender: Loc.GetString("immortal-snail-announcement-sender"),
                        announcementSound: ruleComp.HonkSound,
                        colorOverride: Color.Yellow);
            }

            if (HasComp<GodmodeComponent>(snailUid))
                _godmode.DisableGodmode(snailUid);

            snailComp.Target = null;
            Dirty(snailUid, snailComp);

            return;
        }
    }
}

