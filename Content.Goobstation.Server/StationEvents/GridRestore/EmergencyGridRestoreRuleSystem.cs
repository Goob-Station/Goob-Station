using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Power.SMES;
using Content.Server.Station.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Station.Components;

namespace Content.Goobstation.Server.StationEvents.GridRestore;

public sealed partial class EmergencyGridRestoreRuleSystem : StationEventSystem<EmergencyGridRestoreRuleComponent>
{
    [Dependency] private ApcSystem _apc = default!;
    [Dependency] private SharedBatterySystem _battery = default!;

    protected override void Started(EntityUid uid,
        EmergencyGridRestoreRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var grids = new HashSet<EntityUid>();
        var stations = EntityQueryEnumerator<StationEventEligibleComponent, StationDataComponent>();
        while (stations.MoveNext(out _, out _, out var data))
            grids.UnionWith(data.Grids);

        var apcs = EntityQueryEnumerator<ApcComponent, BatteryComponent, TransformComponent>();
        while (apcs.MoveNext(out var apcUid, out var apc, out var battery, out var xform))
        {
            if (xform.GridUid is not { } grid || !grids.Contains(grid))
                continue;

            if (!apc.MainBreakerEnabled)
                _apc.ApcToggleBreaker(apcUid, apc);

            TopUp((apcUid, battery), component.ApcChargeFraction);
        }

        var smeses = EntityQueryEnumerator<SmesComponent, BatteryComponent, TransformComponent>();
        while (smeses.MoveNext(out var smesUid, out _, out var battery, out var xform))
        {
            if (xform.GridUid is { } grid && grids.Contains(grid))
                TopUp((smesUid, battery), component.SmesChargeFraction);
        }
    }

    private void TopUp(Entity<BatteryComponent> battery, float fraction)
    {
        var target = battery.Comp.MaxCharge * fraction;
        if (_battery.GetCharge((battery, battery.Comp)) < target)
            _battery.SetCharge((battery, battery.Comp), target);
    }
}
