using Content.Goobstation.Common.Kitchen;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class FoodCookedCondition : AchievementCondition;

public sealed class FoodCookedConditionSystem : AchievementConditionSystem<FoodCookedCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, FoodCookedEvent>(OnCooked);
    }

    private void OnCooked(Entity<ActorComponent> ent, ref FoodCookedEvent args)
    {
        ProgressAll(ent, args.Count);
    }
}
