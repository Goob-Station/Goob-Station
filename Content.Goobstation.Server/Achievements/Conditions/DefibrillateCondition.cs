using Content.Shared.Medical;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class DefibrillateCondition : AchievementCondition;

public sealed class DefibrillateConditionSystem : AchievementConditionSystem<DefibrillateCondition>
{
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, TargetDefibrillatedEvent>(OnDefibrillated);
    }

    private void OnDefibrillated(Entity<MobStateComponent> ent, ref TargetDefibrillatedEvent args)
    {
        if (args.User == ent.Owner || _mobState.IsDead(ent, ent.Comp))
            return;

        ProgressAll(args.User);
    }
}
