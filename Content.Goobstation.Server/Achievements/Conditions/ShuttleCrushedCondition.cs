using Content.Goobstation.Common.Shuttles;
using Content.Shared.Body.Components;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class ShuttleCrushedCondition : AchievementCondition;

public sealed class ShuttleCrushedConditionSystem : AchievementConditionSystem<ShuttleCrushedCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, ShuttleCrushedEvent>(OnCrushed);
    }

    private void OnCrushed(Entity<BodyComponent> ent, ref ShuttleCrushedEvent args)
    {
        ProgressAll(ent);
    }
}
