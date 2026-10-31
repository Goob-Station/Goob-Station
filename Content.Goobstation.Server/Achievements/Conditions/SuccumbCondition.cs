using Content.Server.Mobs;
using Content.Shared.Mobs;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class SuccumbCondition : AchievementCondition;

public sealed class SuccumbConditionSystem : AchievementConditionSystem<SuccumbCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, CritSuccumbEvent>(OnSuccumb, before: [typeof(CritMobActionsSystem)]);
    }

    private void OnSuccumb(Entity<ActorComponent> ent, ref CritSuccumbEvent args)
    {
        if (!args.Handled)
            ProgressAll(ent);
    }
}
