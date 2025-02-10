using System.Linq;
using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class WizardRuleSystem : GameRuleSystem<WizardRuleComponent>
{
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public static readonly ProtoId<NpcFactionPrototype> Faction = "Wizard";

    public static readonly EntProtoId Role = "MindRoleWizard";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardRuleComponent, AfterAntagEntitySelectedEvent>(OnAfterAntagSelected);

        SubscribeLocalEvent<WizardRoleComponent, GetBriefingEvent>(OnWizardGetBriefing);
        SubscribeLocalEvent<ApprenticeRoleComponent, GetBriefingEvent>(OnApprenticeGetBriefing);

        SubscribeLocalEvent<WizardComponent, MobStateChangedEvent>(OnStateChanged);
        SubscribeLocalEvent<WizardComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<ApprenticeComponent, MobStateChangedEvent>(OnStateChanged);
        SubscribeLocalEvent<ApprenticeComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<PhylacteryComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<EntParentChangedMessage>(OnParentChanged);
    }

    private void OnParentChanged(ref EntParentChangedMessage args)
    {
        if (args.OldMapId != args.Transform.MapUid && RecursivePhylacteryCheck(args.Entity, args.Transform))
            CheckRoundShouldEnd();
    }

    private bool RecursivePhylacteryCheck(EntityUid entity, TransformComponent? xform = null)
    {
        if (HasComp<PhylacteryComponent>(entity))
            return true;

        if (!Resolve(entity, ref xform, false))
            return false;

        var enumerator = xform.ChildEnumerator;
        while (enumerator.MoveNext(out var child))
        {
            if (RecursivePhylacteryCheck(child))
                return true;
        }

        return false;
    }

    private void OnRemove(EntityUid uid, Component component, ComponentRemove args)
    {
        CheckRoundShouldEnd();
    }

    private void OnStateChanged(EntityUid uid, Component component, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            CheckRoundShouldEnd();
    }

    private void CheckRoundShouldEnd()
    {
        if (!_gameTicker.IsGameRuleActive<EndRoundOnWizardDeathRuleComponent>() ||
            !_gameTicker.IsGameRuleActive<WizardRuleComponent>())
            return;

        var endRound = false;
        var query = EntityQueryEnumerator<MindComponent>();
        while (query.MoveNext(out var mind, out var mindComp))
        {
            if (!_role.MindHasRole<WizardRoleComponent>(mind) && !_role.MindHasRole<ApprenticeRoleComponent>(mind))
                continue;

            if (!_mind.IsCharacterDeadIc(mindComp))
                return;

            if (TryComp(mindComp.OwnedEntity, out MobStateComponent? mobState) &&
                mobState.CurrentState != MobState.Dead)
                return;

            if (TryComp(mind, out SoulBoundComponent? soulBound) && Exists(soulBound.Item) &&
                HasComp<PhylacteryComponent>(soulBound.Item.Value) &&
                TryComp(soulBound.Item.Value, out TransformComponent? xform) && xform.MapUid != null &&
                xform.MapUid == soulBound.MapId)
                return;

            endRound = true;
        }

        if (!endRound)
            return;

        var endQuery = EntityQueryEnumerator<EndRoundOnWizardDeathRuleComponent, GameRuleComponent>();
        while (endQuery.MoveNext(out var uid, out _, out var gameRule))
        {
            _gameTicker.EndGameRule(uid, gameRule);
        }
        _roundEnd.EndRound();
    }

    public IEnumerable<Entity<StationDataComponent>> GetWizardTargetStations()
    {
        var query = EntityQueryEnumerator<StationWizardTargetComponent, StationDataComponent>();
        while (query.MoveNext(out var station, out _, out var data))
        {
            yield return (station, data);
        }
    }

    public IEnumerable<EntityUid?> GetWizardTargetStationGrids()
    {
        return GetWizardTargetStations().Select(station => _station.GetLargestGrid(station.Comp));
    }

    public EntityUid? GetWizardTargetRandomStationGrid()
    {
        var grids = GetWizardTargetStationGrids().Where(grid => grid != null).ToList();
        return grids.Count == 0 ? null : _random.Pick(grids);
    }

    protected override void Started(EntityUid uid,
        WizardRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        var stations = GetWizardTargetStations().ToList();
        if (stations.Count == 0)
            return;
        component.TargetStation = _random.Pick(stations);
    }

    private void OnWizardGetBriefing(Entity<WizardRoleComponent> ent, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("wizard-role-briefing"));
    }

    private void OnApprenticeGetBriefing(Entity<ApprenticeRoleComponent> ent, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("apprentice-role-briefing"));
    }

    private void OnAfterAntagSelected(Entity<WizardRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        MakeWizard(args.EntityUid, ent.Comp);
    }

    public bool MakeWizard(EntityUid target, WizardRuleComponent rule)
    {
        var station = (rule.TargetStation is not null) ? Name(rule.TargetStation.Value) : "the station";

        _antag.SendBriefing(target, Loc.GetString("wizard-role-greeting", ("station", station)), Color.LightBlue, null);

        if (!TryComp(target, out HumanoidAppearanceComponent? humanoid) || humanoid.Age >= 60)
            return true;

        // Wizards are old
        humanoid.Age = _random.Next(60, 121);
        Dirty(target, humanoid);

        return true;
    }
}
