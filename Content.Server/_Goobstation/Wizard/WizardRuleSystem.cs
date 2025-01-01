using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.Station.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Wizard;

public sealed class WizardRuleSystem : GameRuleSystem<WizardRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardRuleComponent, AfterAntagEntitySelectedEvent>(OnAfterAntagSelected);

        SubscribeLocalEvent<WizardRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    public Entity<StationDataComponent>? GetWizardTargetStation()
    {
        var stations = new List<Entity<StationDataComponent>>();
        var query = EntityQueryEnumerator<StationWizardTargetComponent, StationDataComponent>();
        while (query.MoveNext(out var station, out _, out var data))
        {
            stations.Add((station, data));
        }

        if (stations.Count == 0)
            return null;

        return RobustRandom.Pick(stations);
    }

    protected override void Started(EntityUid uid,
        WizardRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        component.TargetStation = GetWizardTargetStation();
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

        return true;
    }
}
