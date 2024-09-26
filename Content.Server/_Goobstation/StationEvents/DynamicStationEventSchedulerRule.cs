using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.GameTicking.Components;

namespace Content.Server.StationEvents;

public sealed partial class DynamicStationEventSchedulerRule : GameRuleSystem<DynamicStationEventSchedulerComponent>
{
    [Dependency] private readonly DynamicRuleSystem _dynamic = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var events in EntityQuery<DynamicStationEventSchedulerComponent>())
        {
            events.EventClock -= frameTime;
            if (events.EventClock <= 0)
            {
                RollRandomAntagEvent();
                ResetTimer(component);
            }
        }
    }

    public void RollRandomAntagEvent()
    {
        // todo finish
    }

    private void ResetTimer(DynamicStationEventSchedulerComponent component)
    {
        component.EventClock = RobustRandom.NextFloat(component.Delays.Min, component.Delays.Max);
    }

    protected override void Started(EntityUid uid, DynamicStationEventSchedulerComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        ResetTimer(component);

        // get all dynamic budgets at once
        if (component.Budget == null)
            foreach (var dyn in EntityQuery<DynamicRuleComponent>())
                component.Budget += dyn.MidroundBudget;
    }
    protected override void Ended(EntityUid uid, DynamicStationEventSchedulerComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        component.EventClock = component.FirstEventDelay;
    }
}
