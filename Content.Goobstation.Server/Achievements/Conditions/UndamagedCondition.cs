using Content.Server.GameTicking;
using Content.Shared.Damage.Systems;
using Content.Shared.GameTicking;
using Robust.Server.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class UndamagedCondition : AchievementCondition;

public sealed partial class UndamagedConditionSystem : AchievementConditionSystem<UndamagedCondition>
{
    [Dependency] private IPlayerManager _player = default!;

    private readonly HashSet<EntityUid> _eligible = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawned);
        SubscribeLocalEvent<ActorComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnSpawned(PlayerSpawnCompleteEvent ev)
    {
        if (!ev.LateJoin)
            _eligible.Add(ev.Mob);
    }

    private void OnDamageChanged(Entity<ActorComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageIncreased)
            _eligible.Remove(ent);
    }

    private void OnRoundEnd(RoundEndTextAppendEvent ev)
    {
        foreach (var session in _player.Sessions)
        {
            if (session.AttachedEntity is { } player && _eligible.Contains(player))
                ProgressAll(player);
        }
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _eligible.Clear();
    }
}
