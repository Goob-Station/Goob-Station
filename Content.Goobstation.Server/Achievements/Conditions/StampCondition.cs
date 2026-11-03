using Content.Goobstation.Common.Paper;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class StampCondition : AchievementCondition;

public sealed class StampConditionSystem : AchievementConditionSystem<StampCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, PaperStampedEvent>(OnStamped);
    }

    private void OnStamped(Entity<ActorComponent> ent, ref PaperStampedEvent args)
    {
        ProgressAll(ent);
    }
}
