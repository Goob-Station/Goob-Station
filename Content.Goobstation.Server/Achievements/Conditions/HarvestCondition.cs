using Content.Goobstation.Common.Botany;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class HarvestCondition : AchievementCondition;

public sealed class HarvestConditionSystem : AchievementConditionSystem<HarvestCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, PlantHarvestedEvent>(OnHarvested);
    }

    private void OnHarvested(Entity<ActorComponent> ent, ref PlantHarvestedEvent args)
    {
        ProgressAll(ent);
    }
}
