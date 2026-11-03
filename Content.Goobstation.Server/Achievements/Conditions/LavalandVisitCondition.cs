using Content.Shared._Lavaland.Procedural.Components;
using Robust.Server.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class LavalandVisitCondition : AchievementCondition;

public sealed partial class LavalandVisitConditionSystem : AchievementConditionSystem<LavalandVisitCondition>
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPlayerManager _player = default!;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(5);

    private TimeSpan _nextCheck;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (Entries.Count == 0 || _timing.CurTime < _nextCheck)
            return;

        _nextCheck = _timing.CurTime + CheckInterval;

        foreach (var session in _player.Sessions)
        {
            if (session.AttachedEntity is not { } player
                || Transform(player).MapUid is not { } map
                || !HasComp<LavalandMapComponent>(map))
                continue;

            ProgressAll(player);
        }
    }
}
