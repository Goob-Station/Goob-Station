using Content.Shared.EntityConditions;
using Content.Shared.Medical;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class DefibrillateCondition : AchievementCondition
{
    [DataField]
    public bool RequireRevive = true;

    [DataField]
    public bool OnAttempt;

    [DataField]
    public EntityCondition[]? TargetRequirements;
}

public sealed partial class DefibrillateConditionSystem : AchievementConditionSystem<DefibrillateCondition>
{
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedEntityConditionsSystem _conditions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, TargetBeforeDefibrillatorZapsEvent>(OnZapAttempt);
        SubscribeLocalEvent<MobStateComponent, TargetDefibrillatedEvent>(OnDefibrillated);
    }

    private void OnZapAttempt(Entity<MobStateComponent> ent, ref TargetBeforeDefibrillatorZapsEvent args)
    {
        if (args.EntityUsingDefib == ent.Owner || !_mobState.IsDead(ent, ent.Comp))
            return;

        foreach (var (proto, condition) in Entries)
        {
            if (!condition.OnAttempt
                || condition.TargetRequirements is { } requirements && !_conditions.TryConditions(ent, requirements))
                continue;

            Achievements.AddProgress(args.EntityUsingDefib, proto);
        }
    }

    private void OnDefibrillated(Entity<MobStateComponent> ent, ref TargetDefibrillatedEvent args)
    {
        if (args.User == ent.Owner)
            return;

        var revived = !_mobState.IsDead(ent, ent.Comp);
        foreach (var (proto, condition) in Entries)
        {
            if (condition.OnAttempt
                || condition.RequireRevive && !revived
                || condition.TargetRequirements is { } requirements && !_conditions.TryConditions(ent, requirements))
                continue;

            Achievements.AddProgress(args.User, proto);
        }
    }
}
