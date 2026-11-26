using Content.Goobstation.Common.PlayerListener;
using Content.Shared.EntityConditions;
using Content.Shared.GameTicking;
using Content.Shared.Mobs;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class CausedMobStateCondition : AchievementCondition
{
    [DataField]
    public List<MobState> MobStates = new() { MobState.Dead };

    [DataField]
    public EntityCondition[]? TargetRequirements;
}

public sealed partial class KilledBySameCondition : AchievementCondition
{
    [DataField]
    public int Kills = 3;
}

public sealed partial class KillRageQuitCondition : AchievementCondition;

public sealed partial class KillConditionsSystem : EntitySystem
{
    [Dependency] private AchievementSystem _achievements = default!;
    [Dependency] private CausedMobStateConditionSystem _caused = default!;
    [Dependency] private KilledBySameConditionSystem _killedBySame = default!;
    [Dependency] private KillRageQuitConditionSystem _rageQuit = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.Origin is not { } origin || args.NewMobState is not (MobState.Critical or MobState.Dead))
            return;

        var attacker = _achievements.ResolveAttacker(origin);
        if (attacker == args.Target)
            return;

        _caused.OnCaused(attacker, args.Target, args.NewMobState);
        _rageQuit.OnCaused(attacker, args.Target);

        if (args.NewMobState == MobState.Dead)
            _killedBySame.OnKilled(attacker, args.Target);
    }
}

public sealed partial class CausedMobStateConditionSystem : AchievementConditionSystem<CausedMobStateCondition>
{
    [Dependency] private SharedEntityConditionsSystem _conditions = default!;

    public void OnCaused(EntityUid attacker, EntityUid target, MobState state)
    {
        foreach (var (proto, condition) in Entries)
        {
            if (!condition.MobStates.Contains(state)
                || condition.TargetRequirements is { } requirements && !_conditions.TryConditions(target, requirements))
                continue;

            Achievements.AddProgress(attacker, proto);
        }
    }
}

public sealed partial class KilledBySameConditionSystem : AchievementConditionSystem<KilledBySameCondition>
{
    private readonly Dictionary<(NetUserId Victim, NetUserId Killer), int> _kills = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    public void OnKilled(EntityUid killer, EntityUid victim)
    {
        if (Entries.Count == 0
            || !Achievements.TryGetSession(killer, out var killerSession)
            || !Achievements.TryGetSession(victim, out var victimSession)
            || killerSession == victimSession)
            return;

        var key = (victimSession.UserId, killerSession.UserId);
        var count = _kills.GetValueOrDefault(key) + 1;
        _kills[key] = count;

        foreach (var (proto, condition) in Entries)
        {
            if (count == condition.Kills)
                Achievements.AddProgress(victim, proto);
        }
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _kills.Clear();
    }
}

public sealed partial class KillRageQuitConditionSystem : AchievementConditionSystem<KillRageQuitCondition>
{
    private readonly Dictionary<NetUserId, EntityUid> _lastAttacker = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerRageQuitEvent>(OnRageQuit);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    public void OnCaused(EntityUid attacker, EntityUid victim)
    {
        if (Entries.Count > 0 && Achievements.TryGetSession(victim, out var session))
            _lastAttacker[session.UserId] = attacker;
    }

    private void OnRageQuit(PlayerRageQuitEvent ev)
    {
        if (!_lastAttacker.Remove(ev.UserId, out var attacker) || TerminatingOrDeleted(attacker))
            return;

        ProgressAll(attacker);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _lastAttacker.Clear();
    }
}
