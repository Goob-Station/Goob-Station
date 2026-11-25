using Content.Shared.Cloning.Events;
using Content.Shared.Mind.Components;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class ClonedCondition : AchievementCondition;

public sealed class ClonedConditionSystem : AchievementConditionSystem<ClonedCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindContainerComponent, CloningEvent>(OnCloned);
    }

    private void OnCloned(Entity<MindContainerComponent> ent, ref CloningEvent args)
    {
        ProgressAll(ent);
    }
}
