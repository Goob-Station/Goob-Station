using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using JetBrains.Annotations;
using Content.Server._Goobstation.Announcements.Systems; // Goobstation - Custom Announcers
using Robust.Shared.Player; // Goobstation - Custom Announcers

namespace Content.Server.StationEvents.Events;

[UsedImplicitly]
public sealed class BreakerFlipRule : StationEventSystem<BreakerFlipRuleComponent>
{
    [Dependency] private readonly ApcSystem _apcSystem = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!; // Goobstation - Custom Announcers

    protected override void Added(EntityUid uid, BreakerFlipRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        _announcer.SendAnnouncement(
            _announcer.GetAnnouncementId(args.RuleId),
            Filter.Broadcast(),
            "station-event-breaker-flip-announcement",
            null,
            Color.Gold,
            null, null,
            ("data", Loc.GetString(Loc.GetString($"random-sentience-event-data-{RobustRandom.Next(1, 6)}")))
        ); // Goobstation - Custom Announcers

        base.Added(uid, component, gameRule, args);

    }

    protected override void Started(EntityUid uid, BreakerFlipRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

        var stationApcs = new List<Entity<ApcComponent>>();
        var query = EntityQueryEnumerator<ApcComponent, TransformComponent>();
        while (query.MoveNext(out var apcUid, out var apc, out var xform))
        {
            if (apc.MainBreakerEnabled && CompOrNull<StationMemberComponent>(xform.GridUid)?.Station == chosenStation)
            {
                stationApcs.Add((apcUid, apc));
            }
        }

        var toDisable = Math.Min(RobustRandom.Next(3, 7), stationApcs.Count);
        if (toDisable == 0)
            return;

        RobustRandom.Shuffle(stationApcs);

        for (var i = 0; i < toDisable; i++)
        {
            _apcSystem.ApcToggleBreaker(stationApcs[i], stationApcs[i]);
        }
    }
}
