using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Players.PlayTimeTracking;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class PlaytimeCondition : AchievementCondition
{
    [DataField]
    public ProtoId<PlayTimeTrackerPrototype> Tracker = "Overall";
}

public sealed partial class PlaytimeConditionSystem : AchievementConditionSystem<PlaytimeCondition>
{
    [Dependency] private PlayTimeTrackingManager _playTime = default!;

    public override void Initialize()
    {
        base.Initialize();

        _playTime.SessionPlayTimeUpdated += CheckSession;
        Achievements.SessionLoaded += CheckSession;
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _playTime.SessionPlayTimeUpdated -= CheckSession;
        Achievements.SessionLoaded -= CheckSession;
    }

    private void CheckSession(ICommonSession session)
    {
        if (!_playTime.TryGetTrackerTimes(session, out var times))
            return;

        foreach (var (proto, condition) in Entries)
        {
            var hours = (int) times.GetValueOrDefault(condition.Tracker.Id).TotalHours;
            Achievements.SetDerivedProgress(session, proto, hours);
        }
    }
}
