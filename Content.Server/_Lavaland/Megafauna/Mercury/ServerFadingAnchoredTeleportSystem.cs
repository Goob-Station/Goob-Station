using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Server._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Fade out, fade in. Wax on, wax off.
/// This became a bit of a Frankenstein's monster as it grew, so the generic name is meaningless
/// Don't try to use this system.
/// TL DR: Teleport at random location, unless boss is in melee form, then teleport towards player, spawning a target on them
/// and dealing damage as it dashes by spawning entities with damagage on collide along the way
/// </summary>
public sealed class ServerFadingAnchoredTeleportSystem : SharedFadingAnchoredTeleportSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FadingAnchoredTeleportComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FadingAnchoredTeleportComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FadingAnchoredTeleportComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.FadeOutStarted)
            {
                comp.Accumulator += frameTime;
                if (comp.Accumulator < comp.FadeOutTime)
                    continue;

                comp.Accumulator = 0f;
                comp.FadeOutStarted = false;
                DoTeleport(uid, comp);
                Dirty(uid, comp);
                continue;
            }

            if (comp.FadeInStarted)
            {
                if (comp.MoveTarget.HasValue)
                {
                    var target = comp.MoveTarget.Value;

                    var delta = target - Transform(uid).WorldPosition;
                    if (delta.Length() <= 0.3f)
                    {
                        _physics.SetLinearVelocity(uid, Vector2.Zero);
                        comp.MoveTarget = null;
                    }
                    else
                    {
                        _physics.SetLinearVelocity(uid, delta.Normalized() * comp.MoveSpeed);
                    }
                }

                // Periodic damage spawning during a player dash
                var isMelee2 = TryComp<PhaseConversionComponent>(uid, out var phase2) && !phase2.IsRanged;
                if (comp.DashDamagePrototype is not null && comp.MoveTarget.HasValue && isMelee2)
                {
                    comp.DashDamageAccumulator += frameTime;
                    if (comp.DashDamageAccumulator >= comp.DashDamageInterval)
                    {
                        comp.DashDamageAccumulator = 0f;
                        Spawn(comp.DashDamagePrototype, Transform(uid).Coordinates);
                    }
                }

                comp.Accumulator += frameTime;
                if (comp.Accumulator < comp.FadeOutTime)
                    continue;

                comp.Accumulator = 0f;
                comp.FadeInStarted = false;

                // Teleport to the desired coordinate just in case the entity hasn't reached it yet (most likely due to stuff in the way)
                if (comp.MoveTarget.HasValue)
                {
                    _physics.SetLinearVelocity(uid, Vector2.Zero);
                    Transform(uid).WorldPosition = comp.MoveTarget.Value;
                    comp.MoveTarget = null;
                }

                // Spawn landing entity at final position if this was a player dash
                if (comp.DashLandPrototype is not null && isMelee2)
                {
                    Spawn(comp.DashLandPrototype, Transform(uid).Coordinates);
                }

                comp.DashDamageAccumulator = 0f;

                // Remove player target indicator
                if (comp.PlayerTargetEntity.HasValue && Exists(comp.PlayerTargetEntity.Value))
                {
                    QueueDel(comp.PlayerTargetEntity.Value);
                    comp.PlayerTargetEntity = null;
                }

                // Remove trail comp
                if (comp.DashTrail is not null)
                {
                    foreach (var (name, _) in comp.DashTrail)
                    {
                        var type = _factory.GetComponent(name).GetType();
                        RemCompDeferred(uid, type);
                    }
                }

                // Clean up warning indicator if it somehow survived to this point
                if (comp.DashWarningEntity.HasValue && Exists(comp.DashWarningEntity.Value))
                {
                    QueueDel(comp.DashWarningEntity.Value);
                    comp.DashWarningEntity = null;
                }

                Dirty(uid, comp);
                continue;
            }

            comp.Accumulator += frameTime;
            var isMelee = TryComp<PhaseConversionComponent>(uid, out var phase) && !phase.IsRanged;
            if (comp.Accumulator < comp.TeleportDelay * (isMelee ? comp.TeleportDelayMultiplier : 1f))
                continue;

            comp.Accumulator = 0f;
            comp.FadeOutStarted = true;
            Dirty(uid, comp);
        }
    }

    private void DoTeleport(EntityUid uid, FadingAnchoredTeleportComponent comp)
    {
        if (comp.AnchorEntity is null)
            return;

        Vector2 targetPos;

        var isMelee = TryComp<PhaseConversionComponent>(uid, out var phase) && !phase.IsRanged;
        if (isMelee && !IsSolarStormActive(uid))
        {
            var nearest = FindNearestPlayer(uid);
            if (nearest.HasValue)
            {
                targetPos = _transform.GetWorldPosition(nearest.Value);
                comp.DashDamageAccumulator = 0f;

                // Spawn target indicator on the player
                if (comp.PlayerTargetPrototype is not null)
                {
                    if (comp.PlayerTargetEntity.HasValue && Exists(comp.PlayerTargetEntity.Value))
                        QueueDel(comp.PlayerTargetEntity.Value);

                    comp.PlayerTargetEntity = Spawn(comp.PlayerTargetPrototype, Transform(nearest.Value).Coordinates);
                    _transform.SetParent(comp.PlayerTargetEntity.Value, nearest.Value);
                }
            }
            else
            {
                // Fall back to random if no players found
                targetPos = GetRandomOffset(uid, comp);
            }
        }
        else
        {
            // Square
            targetPos = GetRandomOffset(uid, comp);
        }

        if (comp.ShouldPlaySound)
        {
            _audio.PlayPvs(comp.TeleportSound, uid, null);
        }

        // Spawn warning at target, only during melee dashes
        if (comp.DashWarningPrototype is not null)
        {
            if (comp.DashWarningEntity.HasValue && Exists(comp.DashWarningEntity.Value))
                QueueDel(comp.DashWarningEntity.Value);

            comp.DashWarningEntity = Spawn(comp.DashWarningPrototype, Transform(uid).Coordinates);
            _transform.SetWorldPosition(comp.DashWarningEntity.Value, targetPos);
        }

        if (comp.MoveInstead)
        {
            comp.MoveTarget = targetPos;
            // add trail comp
            if (comp.DashTrail is not null)
            {
                EntityManager.AddComponents(uid, comp.DashTrail);
            }
        }
        else
        {
            Transform(uid).Coordinates = Transform(comp.AnchorEntity.Value).Coordinates.Offset(targetPos - _transform.GetWorldPosition(comp.AnchorEntity.Value));
        }

        comp.Accumulator = 0f;
        comp.FadeInStarted = true;
        Dirty(uid, comp);
    }

    // Prevents dashing during solar storm
    private bool IsSolarStormActive(EntityUid uid)
    {
        if (!TryComp<ORTSolarStormComponent>(uid, out var storm))
            return false;

        return storm.IsActive || storm.IsCharging || storm.StormSoon;
    }

    // high spec targetting system (finds nearest player)
    private EntityUid? FindNearestPlayer(EntityUid uid)
    {
        var myPos = _transform.GetWorldPosition(uid);
        var nearest = (EntityUid?) null;
        var nearestDist = float.MaxValue;

        var candidates = new HashSet<EntityUid>();
        _lookup.GetEntitiesInRange(Transform(uid).Coordinates, 30f, candidates);

        foreach (var candidate in candidates)
        {
            if (!HasComp<ActorComponent>(candidate))
                continue;

            var dist = (_transform.GetWorldPosition(candidate) - myPos).Length();
            if (dist >= nearestDist)
                continue;

            nearestDist = dist;
            nearest = candidate;
        }

        return nearest;
    }

    private Vector2 GetRandomOffset(EntityUid uid, FadingAnchoredTeleportComponent comp)
    {
        var anchorPosition = _transform.GetWorldPosition(comp.AnchorEntity!.Value);
        var offset = new Vector2(_random.NextFloat(-comp.TeleportDistance, comp.TeleportDistance), _random.NextFloat(-comp.TeleportDistance, comp.TeleportDistance));
        return anchorPosition + offset;
    }

    private void OnStartup(EntityUid uid, FadingAnchoredTeleportComponent comp, ComponentStartup args)
    {
        var coords = Transform(uid).Coordinates;
        comp.AnchorEntity = Spawn(comp.AnchorPrototype, coords);
    }

    private void OnShutdown(EntityUid uid, FadingAnchoredTeleportComponent comp, ComponentShutdown args)
    {
        if (comp.AnchorEntity.HasValue)
        {
            QueueDel(comp.AnchorEntity.Value);
        }

        if (comp.DashWarningEntity.HasValue && Exists(comp.DashWarningEntity.Value))
        {
            QueueDel(comp.DashWarningEntity.Value);
        }

        if (comp.PlayerTargetEntity.HasValue && Exists(comp.PlayerTargetEntity.Value))
        {
            QueueDel(comp.PlayerTargetEntity.Value);
        }
    }
}
