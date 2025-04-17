using Content.Server._Sunrise.AssaultOps.Icarus;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Revolutionary.Components;
using Content.Server.RoundEnd;
using Content.Server.Station.Components;
using Content.Server.Traitor.Uplink;
using Content.Shared._Sunrise.AssaultOps;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;
using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Server._Sunrise.AssaultOps;

public sealed class AssaultOpsRuleSystem : GameRuleSystem<AssaultOpsRuleComponent>
{
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedSubdermalImplantSystem _subdermalImplant = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly UplinkSystem _uplinkSystem = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;

    [ValidatePrototypeId<TagPrototype>]
    private const string UplinkTagPrototype = "AssaultOpsUplink";

    [ValidatePrototypeId<AntagPrototype>]
    private const string CommanderAntagProto = "AssaultCommander";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawned);
        SubscribeLocalEvent<IcarusTerminalSystem.IcarusActivatedEvent>(OnIcarusActivated);
        SubscribeLocalEvent<AssaultOperativeComponent, MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<AssaultOpsRuleComponent, AfterAntagEntitySelectedEvent>(OnAfterAntagEntSelected);
        SubscribeLocalEvent<AssaultOpsRuleComponent, AntagSelectionCompleteEvent>(OnAfterAntagSelectionComplete); // Sunrise-Edit
        SubscribeLocalEvent<AssaultOpsRuleComponent, RuleLoadedGridsEvent>(OnRuleLoadedGrids);
    }

    protected override void Started(EntityUid uid,
        AssaultOpsRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        var eligible = new List<Entity<StationEventEligibleComponent, NpcFactionMemberComponent>>();
        var eligibleQuery = EntityQueryEnumerator<StationEventEligibleComponent, NpcFactionMemberComponent>();
        while (eligibleQuery.MoveNext(out var eligibleUid, out var eligibleComp, out var member))
        {
            if (!_npcFaction.IsFactionHostile(component.Faction, (eligibleUid, member)))
                continue;

            eligible.Add((eligibleUid, eligibleComp, member));
        }

        if (eligible.Count == 0)
            return;

        component.TargetStation = RobustRandom.Pick(eligible);

        if (GameTicker.RunLevel == GameRunLevel.InRound)
        {
            InsertIcarusKeys(uid, component);
        }
    }

    private void OnRuleLoadedGrids(Entity<AssaultOpsRuleComponent> ent, ref RuleLoadedGridsEvent args)
    {
        var query = EntityQueryEnumerator<AssaultOpsShuttleComponent>();
        while (query.MoveNext(out var uid, out var shuttle))
        {
            if (Transform(uid).MapID == args.Map)
            {
                shuttle.AssociatedRule = ent;
                break;
            }
        }
    }

    private void OnAfterAntagEntSelected(Entity<AssaultOpsRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        var target = (ent.Comp.TargetStation is not null) ? Name(ent.Comp.TargetStation.Value) : "the target";

        _antag.SendBriefing(args.Session,
            Loc.GetString("assaultops-welcome",
                ("station", target),
                ("name", Name(ent))),
            Color.Red,
            ent.Comp.GreetSoundNotification);

        // Sunrise-Start
        if (!args.GameRule.Comp.UseSpawners)
            return;

        ent.Comp.RoundstartOperatives = args.GameRule.Comp.SpawnersCount;
        var commander = GetCommander(args.GameRule);
        if (commander != null)
            SetupUplink(commander.Value, ent.Comp);
        // Sunrise-End
    }

    private void OnAfterAntagSelectionComplete(Entity<AssaultOpsRuleComponent> ent, ref AntagSelectionCompleteEvent args)
    {
        ent.Comp.RoundstartOperatives = args.GameRule.Comp.SelectedMinds.Count;

        var commander = GetCommander(args.GameRule);
        if (commander != null)
            SetupUplink(commander.Value, ent.Comp);
    }

    private EntityUid? GetCommander(Entity<AntagSelectionComponent> antagSelection)
    {
        EntityUid? commander = null;
        foreach (var compSelectedMind in antagSelection.Comp.SelectedMinds)
        {
            if (!TryComp<MindComponent>(compSelectedMind.Item1, out var mindComp))
                continue;

            foreach (var roleInfo in _roles.MindGetAllRoleInfo((compSelectedMind.Item1, mindComp)))
            {
                if (roleInfo.Prototype != CommanderAntagProto || mindComp.CurrentEntity == null)
                    continue;

                commander = mindComp.CurrentEntity.Value;
            }
        }

        return commander;
    }

    private void SetupUplink(EntityUid user, AssaultOpsRuleComponent rule)
    {
        var uplink = _uplinkSystem.FindUplinkByTag(user, UplinkTagPrototype);
        if (uplink != null)
            _uplinkSystem.SetUplink(user, uplink.Value, rule.TCAmountPerOperative * rule.RoundstartOperatives, true);
    }

    private void InsertIcarusKeys(EntityUid uid, AssaultOpsRuleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var query = EntityQueryEnumerator<CommandStaffComponent>();
        while (query.MoveNext(out var ent, out var mind))
        {
            var haveKey = false;
            if (_container.TryGetContainer(ent, ImplanterComponent.ImplantSlotId, out var implantContainer))
            {
                foreach (var implant in implantContainer.ContainedEntities)
                {
                    if (MetaData(implant).EntityPrototype!.ID == component.IcarusKeyImplant)
                        haveKey = true;
                }
            }
            if (!haveKey)
                InsertKey(ent, component.IcarusKeyImplant);
        }
    }

    private bool InsertKey(EntityUid uid, string icarusKeyImplant)
    {
        var ownedCoords = Transform(uid).Coordinates;
        var implant = Spawn(icarusKeyImplant, ownedCoords);

        if (!TryComp<SubdermalImplantComponent>(implant, out var implantComp))
            return false;

        _subdermalImplant.ForceImplant(uid, implant, implantComp);
        return true;
    }

    private void OnIcarusActivated(IcarusTerminalSystem.IcarusActivatedEvent ev)
    {
        var query = EntityQueryEnumerator<AssaultOpsRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var assaultops, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
            {
                Logger.Info("AssaultopsRule not added");
                continue;
            }
            assaultops.WinConditions.Add(WinCondition.IcarusActivated);
        }
    }

    private void OnPlayerSpawned(PlayerSpawnCompleteEvent ev)
    {
        var query = EntityQueryEnumerator<AssaultOpsRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var assaultops, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            var session = ev.Player;

            var mind = _mind.GetMind(session.UserId);

            if (mind == null)
                continue;

            if (HasComp<CommandStaffComponent>(ev.Mob))
                InsertKey(ev.Mob, assaultops.IcarusKeyImplant);
        }
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        if (ev.New is not GameRunLevel.PostRound)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var rule, out _))
        {
            OnRoundEnd(uid, rule);
        }
    }

    private void OnRoundEnd(EntityUid uid, AssaultOpsRuleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var total = 0;
        var alive = 0;
        foreach (var (_, state) in EntityQuery<AssaultOperativeComponent, MobStateComponent>())
        {
            total++;
            if (state.CurrentState != MobState.Alive)
                continue;

            alive++;
            break;
        }

        var allAlive = alive == total;
        if (allAlive)
        {
            if (component.WinConditions.Contains(WinCondition.IcarusActivated))
            {
                component.WinType = WinType.OpsMajor;
            }
            else
            {
                component.WinType = WinType.OpsMinor;
                component.WinConditions.Add(WinCondition.AllOpsAlive);
            }
        }
        else if (alive == 0)
        {
            if (component.WinConditions.Contains(WinCondition.IcarusActivated))
            {
                component.WinType = WinType.Hearty;
            }
            else
            {
                component.WinType = WinType.CrewMajor;
                component.WinConditions.Add(WinCondition.AllOpsDead);
            }
        }
        else
        {
            if (component.WinConditions.Contains(WinCondition.IcarusActivated))
            {
                component.WinType = WinType.OpsMinor;
            }
            else
            {
                component.WinType = WinType.Stalemate;
                component.WinConditions.Add(WinCondition.SomeOpsAlive);
            }
        }
    }

    private void OnMobStateChanged(EntityUid uid, AssaultOperativeComponent component, MobStateChangedEvent ev)
    {
        if(ev.NewMobState == MobState.Dead)
            CheckRoundShouldEnd();
    }

    private void CheckRoundShouldEnd()
    {
        var query = EntityQueryEnumerator<AssaultOpsRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var assaultops, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            if (assaultops.WinType == WinType.CrewMajor || assaultops.WinType == WinType.OpsMajor)
                continue;

            var operativesAlive = false;
            var operatives = EntityQuery<AssaultOperativeComponent, MobStateComponent>(true);
            foreach (var (assaultOp, mobState) in operatives)
            {
                if (mobState.CurrentState is MobState.Alive)
                {
                    operativesAlive = true;
                }
            }

            if (operativesAlive)
                continue;

            _roundEndSystem.EndRound();
        }
    }

    protected override void AppendRoundEndText(EntityUid uid,
        AssaultOpsRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        var winText = Loc.GetString($"assaultops-{component.WinType.ToString().ToLower()}");
        args.AddLine(winText);

        foreach (var cond in component.WinConditions)
        {
            var text = Loc.GetString($"assaultops-cond-{cond.ToString().ToLower()}");
            args.AddLine(text);
        }

        args.AddLine(Loc.GetString("assaultops-list-start"));

        var antags =_antag.GetAntagIdentifiers(uid);

        foreach (var (_, sessionData, name) in antags)
        {
            args.AddLine(Loc.GetString("assaultops-list-name", ("name", name), ("user", sessionData.UserName)));
        }
    }
}
