using Content.Shared.Doors.Components;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class WeldDoorCondition : AchievementCondition;

public sealed class WeldDoorConditionSystem : AchievementConditionSystem<WeldDoorCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DoorComponent, WeldFinishedEvent>(OnWeldFinished, after: [typeof(WeldableSystem)]);
    }

    private void OnWeldFinished(Entity<DoorComponent> ent, ref WeldFinishedEvent args)
    {
        if (args.Cancelled || !TryComp<WeldableComponent>(ent, out var weldable) || !weldable.IsWelded)
            return;

        ProgressAll(args.User);
    }
}
