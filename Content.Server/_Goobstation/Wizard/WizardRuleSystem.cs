using System.Linq;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Roles;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Wizard;

public sealed class WizardRuleSystem : GameRuleSystem<WizardRuleComponent>
{
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public static readonly ProtoId<NpcFactionPrototype> Faction = "Wizard";

    public static readonly EntProtoId Role = "MindRoleWizard";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardRuleComponent, AfterAntagEntitySelectedEvent>(OnAfterAntagSelected);

        SubscribeLocalEvent<WizardRoleComponent, GetBriefingEvent>(OnGetBriefing);
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

    private void OnGetBriefing(Entity<WizardRoleComponent> ent, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("wizard-role-briefing"));
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
