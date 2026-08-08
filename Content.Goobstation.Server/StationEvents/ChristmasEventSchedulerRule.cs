// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;

namespace Content.Goobstation.Server.StationEvents;

public sealed class ChristmasEventSchedulerRule : GameRuleSystem<ChristmasEventSchedulerComponent>
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var events in EntityQuery<ChristmasEventSchedulerComponent>())
        {
            events.EventClock -= frameTime;
            if (events.EventClock <= 0)
            {
                //RollRandomChristmasEvent(events);
                ResetTimer(events);
            }
        }
    }

    private void ResetTimer(ChristmasEventSchedulerComponent component)
    {
        component.EventClock = RobustRandom.NextFloat(component.Delays.Min, component.Delays.Max);
    }

    protected override void Started(EntityUid uid, ChristmasEventSchedulerComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        ResetTimer(component);
    }
    protected override void Ended(EntityUid uid, ChristmasEventSchedulerComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        ResetTimer(component);
    }
}
