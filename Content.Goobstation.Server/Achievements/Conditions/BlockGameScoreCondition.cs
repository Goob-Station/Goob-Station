using Content.Goobstation.Common.Arcade;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class BlockGameScoreCondition : AchievementCondition
{
    [DataField(required: true)]
    public int Score;
}

public sealed class BlockGameScoreConditionSystem : AchievementConditionSystem<BlockGameScoreCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, BlockGameOverEvent>(OnGameOver);
    }

    private void OnGameOver(Entity<ActorComponent> ent, ref BlockGameOverEvent args)
    {
        foreach (var (proto, condition) in Entries)
        {
            if (args.Points >= condition.Score)
                Achievements.AddProgress(ent, proto);
        }
    }
}
