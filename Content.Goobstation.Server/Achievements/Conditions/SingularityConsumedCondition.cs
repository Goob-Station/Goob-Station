using Content.Server.Singularity.Events;
using Content.Shared.Mobs.Components;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class SingularityConsumedCondition : AchievementCondition;

public sealed class SingularityConsumedConditionSystem : AchievementConditionSystem<SingularityConsumedCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, EventHorizonConsumedEntityEvent>(OnConsumed);
    }

    private void OnConsumed(Entity<MobStateComponent> ent, ref EventHorizonConsumedEntityEvent args)
    {
        ProgressAll(ent);
    }
}
