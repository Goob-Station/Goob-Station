using Content.Server._White.GameTicking.Rules;
using Content.Server.GameTicking.Rules;
using Content.Server.Nuke;
using Content.Server.Station.Components;
using Content.Shared.GameTicking;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Objectives;

public sealed class DetonateNukeObjectiveSystem : EntitySystem
{
    private bool _stationNuked;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DetonateNukeConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);

        SubscribeLocalEvent<NukeExplodedEvent>(OnNuke, before: [typeof(XenomorphsRuleSystem), typeof(NukeopsRuleSystem)]);
    }

    private void OnNuke(NukeExplodedEvent ev)
    {
        if (HasComp<BecomesStationComponent>(ev.OwningStation))
            _stationNuked = true;
    }

    private void OnRestart(RoundRestartCleanupEvent ev)
    {
        _stationNuked = false;
    }

    private void OnGetProgress(Entity<DetonateNukeConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = _stationNuked ? 1f : 0f;
    }
}
