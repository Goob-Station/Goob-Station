using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;

namespace Content.Server.StationEvents;

public sealed partial class DynamicStationEventSchedulerRule : GameRuleSystem<DynamicStationEventSchedulerComponent>
{
    [Dependency] private readonly DynamicRuleSystem _dynamic = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var events in EntityQuery<DynamicStationEventSchedulerComponent>())
        {
            events.EventClock -= frameTime;
            if (events.EventClock <= 0)
            {
                RollRandomAntagEvent(events);
                ResetTimer(events);
            }
        }
    }

    public void RollRandomAntagEvent(DynamicStationEventSchedulerComponent component, float attempt = 0, float attemptLimit = 5)
    {
        var rules = _dynamic.GetRulesets(component.MidroundRulesPool);
        var toReroll = false;

        var pickedRule = _dynamic.WeightedPickRule(rules);
        if (pickedRule == null)
            return; // should never happen

        // cancel multiple high impacts
        if (pickedRule.DynamicRuleset.HighImpact
        && component.ExecutedRules.Contains(pickedRule.Prototype.ID))
            toReroll = true;

        // check budget
        var budget = component.Budget - pickedRule.DynamicRuleset.Cost;
        if (budget < 0)
            toReroll = true;

        // try rerolling events until success
        if (toReroll)
        {
            if (attempt >= attemptLimit)
            {
                _adminLog.Add(LogType.EventRan, LogImpact.Low, $"Could not spawn another midround event due to lack of budget.");
                return;
            }

            RollRandomAntagEvent(component, attempt: attempt += 1);
            return;
        }

        // start game rule
        component.Budget = budget;
        _gameTicker.StartGameRule(pickedRule.Prototype.ID);
        component.ExecutedRules.Add(pickedRule.Prototype.ID);
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
        component.Budget = 0;
        foreach (var dyn in EntityQuery<DynamicRuleComponent>())
            component.Budget += dyn.MidroundBudget;
    }
    protected override void Ended(EntityUid uid, DynamicStationEventSchedulerComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        ResetTimer(component);
    }
}
