using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Starts increasing brightness, spawning particles as it does so.
/// Once fully charged, play sound, and after a delay, start dealing damage in a radius.
/// </summary>
public sealed class ORTSolarStormSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPointLightSystem _lights = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ORTSolarStormComponent, ORTSolarStormActionEvent>(OnActionUsed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ORTSolarStormComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.IsCharging)
            {
                comp.Accumulator += frameTime;
                comp.AccumulatorButCooler += frameTime;

                // Particles
                if (comp.AccumulatorButCooler >= comp.CurrentParticleSpawnRate)
                {
                    if (!_net.IsServer)
                        continue;

                    comp.AccumulatorButCooler = 0f;

                    var angle = new Angle(_random.NextDouble() * Math.Tau);
                    var spawnPosition = Transform(uid).Coordinates.Offset(angle.ToVec() * comp.ParticleSpawnRadius);
                    Spawn(comp.ParticlePrototype, spawnPosition);

                    // Spawn them more the longer it charges
                    comp.CurrentParticleSpawnRate = MathF.Max(0.05f, comp.CurrentParticleSpawnRate - comp.ParticleIncreaseBy);
                }

                // Increase the brightness of the warning entity
                if (_timing.CurTime >= comp.NextUpdate && comp.WarningEntity.HasValue)
                {
                    comp.NextUpdate = _timing.CurTime + comp.Interval;
                    var light = _lights.EnsureLight(comp.WarningEntity.Value);
                    _lights.SetColor(comp.WarningEntity.Value, comp.LightColor, light);
                    _lights.SetEnergy(comp.WarningEntity.Value, comp.CurrentGlow + comp.IncreaseBy, light);
                    _lights.SetRadius(comp.WarningEntity.Value, comp.CurrentGlow + comp.IncreaseBy, light);
                    comp.CurrentGlow = MathF.Min(comp.CurrentGlow + comp.IncreaseBy, comp.GlowIntensity);
                }

                if (comp.Accumulator >= comp.ChargeTime)
                {
                    comp.Accumulator = 0f;
                    comp.AccumulatorButCooler = 0f;
                    comp.CurrentParticleSpawnRate = comp.ParticleSpawnRate;
                    comp.IsCharging = false;

                    // Despawn the warning ent and play sound
                    if (comp.WarningEntity.HasValue)
                    {
                        QueueDel(comp.WarningEntity.Value);
                        comp.WarningEntity = null;
                    }

                    comp.StormSoon = true;
                }
            }

            // Small delay before the pain
            if (comp.StormSoon)
            {
                comp.AccumulatorBeforeStorm += frameTime;

                if (comp.AccumulatorBeforeStorm >= comp.WaitForIt)
                {
                    comp.AccumulatorBeforeStorm = 0f;
                    comp.StormSoon = false;
                    DoSolarStorm(uid, comp);
                }
            }

            if (comp.IsActive)
            {
                comp.StormAccumulator += frameTime;
                comp.AccumulatorButLame += frameTime;

                if (comp.AccumulatorButLame >= 0.5f)
                {
                    comp.AccumulatorButLame = 0f;
                    foreach (var target in _lookup.GetEntitiesInRange(Transform(uid).Coordinates, comp.StormRadius))
                    {
                        if (target == uid)
                            continue;

                        if (_mobState.IsDead(target))
                            continue;

                        _damageable.TryChangeDamage(target, comp.StormDamage);
                    }
                }

                if (comp.StormAccumulator >= comp.StormDuration)
                {
                    // reset
                    comp.StormAccumulator = 0f;
                    comp.AccumulatorButLame = 0f;
                    comp.CurrentGlow = 0f;
                    comp.IsActive = false;

                    if (comp.StormEntity.HasValue)
                    {
                        QueueDel(comp.StormEntity.Value);
                        comp.StormEntity = null;
                    }
                }
            }
        }
    }

    private void DoSolarStorm(EntityUid uid, ORTSolarStormComponent comp)
    {
        comp.StormEntity = PredictedSpawnAttachedTo(comp.StormPrototype, Transform(uid).Coordinates);
        _audio.PlayPredicted(comp.IMMAFIRINGMYLASERSound, uid, uid);
        if (comp.StormEntity.HasValue)
        {
            _transform.SetParent(comp.StormEntity.Value, uid);
        }
        comp.IsActive = true;
    }

    private void OnActionUsed(EntityUid uid, ORTSolarStormComponent comp, ORTSolarStormActionEvent args)
    {
        if (args.Handled)
            return;

        // so it doesn't try to charge mid already charging or firing
        if (comp.IsCharging || comp.StormSoon || comp.IsActive)
            return;

        comp.CurrentParticleSpawnRate = comp.ParticleSpawnRate;
        comp.WarningEntity = PredictedSpawnAttachedTo(comp.WarningPrototype, Transform(uid).Coordinates);
        // Because SpawnAttachedTo is a filthy LIAR!!!!!!!!! Why would I want it attached to the damn coordinate instead of the entity?
        if (comp.WarningEntity.HasValue)
        {
            _transform.SetParent(comp.WarningEntity.Value, uid);
        }
        // This is so fucking loud oh lord
        _audio.PlayPredicted(comp.ChargeSound, uid, uid, AudioParams.Default.WithVolume(-5f));
        comp.IsCharging = true;

        args.Handled = true;
    }
}
