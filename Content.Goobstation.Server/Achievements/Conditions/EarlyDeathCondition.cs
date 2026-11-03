using Content.Shared.GameTicking;
using Content.Shared.Mobs;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class EarlyDeathCondition : AchievementCondition
{
    [DataField]
    public TimeSpan Within = TimeSpan.FromSeconds(30);
}

public sealed partial class EarlyDeathConditionSystem : AchievementConditionSystem<EarlyDeathCondition>
{
    [Dependency] private IGameTiming _timing = default!;

    private readonly Dictionary<EntityUid, TimeSpan> _spawnTimes = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawned);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnSpawned(PlayerSpawnCompleteEvent ev)
    {
        _spawnTimes[ev.Mob] = _timing.CurTime;
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || !_spawnTimes.Remove(args.Target, out var spawned))
            return;

        var alive = _timing.CurTime - spawned;
        foreach (var (proto, condition) in Entries)
        {
            if (alive <= condition.Within)
                Achievements.AddProgress(args.Target, proto);
        }
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _spawnTimes.Clear();
    }
}
