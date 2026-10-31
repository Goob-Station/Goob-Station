using Content.Server.GameTicking;
using Robust.Server.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class RoundEndCondition : AchievementCondition
{
    [DataField]
    public TimeSpan? MaxRoundDuration;
}

public sealed partial class RoundEndConditionSystem : AchievementConditionSystem<RoundEndCondition>
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private GameTicker _ticker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundEndTextAppendEvent ev)
    {
        var duration = _ticker.RoundDuration();
        foreach (var session in _player.Sessions)
        {
            if (session.AttachedEntity is not { } player)
                continue;

            foreach (var (proto, condition) in Entries)
            {
                if (condition.MaxRoundDuration is { } max && duration > max)
                    continue;

                Achievements.AddProgress(player, proto);
            }
        }
    }
}
