using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;

namespace Content.Server.StationEvents;

public sealed partial class DynamicStationEventSchedulerRule : GameRuleSystem<DynamicStationEventSchedulerComponent>
{
    [Dependency] private readonly DynamicRuleSystem _dynamic = default!;

    protected override void Added(EntityUid uid, DynamicStationEventSchedulerComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);


    }
}
