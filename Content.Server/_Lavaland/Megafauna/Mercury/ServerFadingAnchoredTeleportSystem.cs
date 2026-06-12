using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Server._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Fade out, fade in. Wax on, wax off.
/// It also teleports you, of course.
/// </summary>

public sealed class ServerFadingAnchoredTeleportSystem : SharedFadingAnchoredTeleportSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;

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

                // Remove trail comp
                if (comp.DashTrail is not null)
                {
                    foreach (var (name, _) in comp.DashTrail)
                    {
                        var type = _factory.GetComponent(name).GetType();
                        RemCompDeferred(uid, type);
                    }
                }
                Dirty(uid, comp);
                continue;
            }

            comp.Accumulator += frameTime;
            if (comp.Accumulator < comp.TeleportDelay)
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

        var anchorPosition = Transform(comp.AnchorEntity.Value).Coordinates;

        // Square
        var offset = new Vector2(_random.NextFloat(-comp.TeleportDistance, comp.TeleportDistance), _random.NextFloat(-comp.TeleportDistance, comp.TeleportDistance));

        if (comp.ShouldPlaySound)
        {
            _audio.PlayPvs(comp.TeleportSound, uid, null);
        }

        if (comp.MoveInstead)
        {
            comp.MoveTarget = Transform(comp.AnchorEntity.Value).WorldPosition + offset;
            // add trail comp
            if (comp.DashTrail is not null)
            {
                EntityManager.AddComponents(uid, comp.DashTrail);
            }
        }
        else
        {
            Transform(uid).Coordinates = anchorPosition.Offset(offset);
        }

        comp.Accumulator = 0f;
        comp.FadeInStarted = true;
        Dirty(uid, comp);
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
    }
}

