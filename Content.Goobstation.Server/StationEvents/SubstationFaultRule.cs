using System.Diagnostics;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Server.Power.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.Tag;
using JetBrains.Annotations;

namespace Content.Goobstation.Server.StationEvents;

/// <summary>
/// This handles...
/// </summary>

[UsedImplicitly]
public sealed class SubstationFaultRule : StationEventSystem<SubstationFaultRuleComponent>
{
    [Dependency] private readonly TagSystem _tag = default!;

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

        if (!TryGetRandomStation(out var chosenStation))
            return;

        var stationSubstations = new List<Entity<PowerNetworkBatteryComponent>>();
        var query = EntityQueryEnumerator<PowerNetworkBatteryComponent, TransformComponent>();
        while (query.MoveNext(out var powerUid, out var power, out var xform))
        {
           if( _tag.HasTag(powerUid,"Substation") &&
               power.CanCharge &&
               power.CanDischarge &&
               xform.Anchored
               )
               stationSubstations.Add((powerUid,power));
        }

        var toDisable = Math.Min( Math.Max(stationSubstations.Count/10, 1), stationSubstations.Count);
        if (toDisable == 0)
            return;

        RobustRandom.Shuffle(stationSubstations);

        for (var i = 0; i < toDisable; i++)
        {
            if(RobustRandom.Next(1, 2) == 2)
                stationSubstations[i].Comp.CanCharge = false;
            else
                stationSubstations[i].Comp.CanDischarge = false;
        }

    }
}
