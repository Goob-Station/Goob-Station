using Content.Shared.GameTicking;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class SpawnCondition : AchievementCondition
{
    [DataField]
    public int? Month;

    [DataField]
    public int? Day;
}

public sealed class SpawnConditionSystem : AchievementConditionSystem<SpawnCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        var now = DateTime.UtcNow;
        foreach (var (proto, condition) in Entries)
        {
            if (condition.Month is { } month && month != now.Month
                || condition.Day is { } day && day != now.Day)
                continue;

            Achievements.AddProgress(ev.Mob, proto);
        }
    }
}
