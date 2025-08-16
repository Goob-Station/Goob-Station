using Content.Goobstation.Server.StationEvents.Components;
using Content.Server.Power.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Server.StationEvents.Events;
using JetBrains.Annotations;

namespace Content.Goobstation.Server.StationEvents;

/// <summary>
/// game event where substations get turned off  randomly
/// </summary>

[UsedImplicitly]
public sealed class SubstationFaultRule : StationEventSystem<SubstationFaultRuleComponent>
{
    protected override void Added(EntityUid uid, SubstationFaultRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        var str = Loc.GetString("station-event-substation-fault-announcement", ("data", Loc.GetString(Loc.GetString($"random-sentience-event-data-{RobustRandom.Next(1, 6)}"))));
        stationEvent.StartAnnouncement = str;

        base.Added(uid, component, gameRule, args);
    }

    protected override void Started(EntityUid uid,
        SubstationFaultRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var _))
            return;

        var stationSubstations = new List<Entity<PowerNetworkBatteryComponent>>();
        var query = EntityQueryEnumerator<SubstationCanFailComponent,PowerNetworkBatteryComponent, TransformComponent>();
        while (query.MoveNext(out var powerUid,out var fail, out var power, out var xform))
        {
           if( fail.CanBeDeactivated && // does not target debug substations
               power.CanCharge &&   // targets must be operating normal
               power.CanDischarge &&
               xform.Anchored       // only targets anchored substations
               )
               stationSubstations.Add((powerUid,power));
        }

        // amount of substations to disable
        var toDisable = Math.Min( RobustRandom.Next(1, 3), stationSubstations.Count);
        if (toDisable == 0)
            return;

        RobustRandom.Shuffle(stationSubstations);

        for (var i = 0; i < toDisable; i++)
        {
            if(RobustRandom.Next(1, 2) == 2) // chance of turning off the input or the output
                stationSubstations[i].Comp.CanCharge = false;
            else
                stationSubstations[i].Comp.CanDischarge = false;
        }
    }
}
