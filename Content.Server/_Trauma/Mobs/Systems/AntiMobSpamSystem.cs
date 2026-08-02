using Content.Shared._Trauma.Utility;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Trauma.Shared.Mobs;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Content.Goobstation.Common.CCVar;

namespace Content.Server._Trauma.Mobs.Systems;

/// <summary>
/// Makes entities with <see cref="MobSpamSystem"/> despawn 5 minutes after dying.
/// </summary>
public sealed partial class MobSpamSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public static readonly TimeSpan DespawnTime = TimeSpan.FromMinutes(5);

    private TimedRingBuffer<EntityUid> _buffer = default!;

    public override void Initialize()
    {
        base.Initialize();

        _buffer = new(64, DespawnTime, _timing);

        Subs.CVar(_cfg, GoobCVars.AntiMobSpamUpdateRate, x => _buffer.PopDelay = TimeSpan.FromMinutes(x), true);

        SubscribeLocalEvent<AntiMobSpamComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_buffer.PopNext(out var uid))
            Despawn(uid);
    }

    private void OnMobStateChanged(Entity<AntiMobSpamComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        QueueDespawn(ent);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _buffer.Reset();
    }

    private void QueueDespawn(EntityUid uid)
    {
        if (_buffer.Push(uid, out var old))
            Despawn(old);
    }

    private void Despawn(EntityUid uid)
    {
        // don't delete mobs that got revived
        if (TerminatingOrDeleted(uid) || !_mob.IsDead(uid))
            return;

        QueueDel(uid);
    }
}
