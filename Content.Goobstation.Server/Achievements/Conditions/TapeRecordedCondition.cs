using Content.Goobstation.Shared.TapeRecorder;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class TapeRecordedCondition : AchievementCondition;

public sealed class TapeRecordedConditionSystem : AchievementConditionSystem<TapeRecordedCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, TapeRecordedEvent>(OnRecorded);
    }

    private void OnRecorded(Entity<ActorComponent> ent, ref TapeRecordedEvent args)
    {
        ProgressAll(ent);
    }
}
