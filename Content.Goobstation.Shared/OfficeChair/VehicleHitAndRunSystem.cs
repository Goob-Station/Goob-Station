// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Body.Components;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.OfficeChair;

public sealed partial class VehicleHitAndRunSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (!_net.IsServer)
            return;

        var now = _timing.CurTime;

        var query = EntityQueryEnumerator<VehicleHitAndRunComponent, PhysicsComponent>();
        while (query.MoveNext(out var uid, out var comp, out var physics))
        {
            if (!comp.CanRunOver)
                continue;

            var speed = physics.LinearVelocity.Length();
            if (speed < comp.MinRunoverSpeed)
                continue;

            var pos = _xform.GetMapCoordinates(uid);
            var fwd = physics.LinearVelocity.LengthSquared() > 0.0001f ? Vector2.Normalize(physics.LinearVelocity) : Vector2.Normalize(Vector2.UnitX);
            var threwAny = false;

            EntityUid? ignore = null;
            if (TryComp<RocketChairComponent>(uid, out var chair2))
                ignore = chair2.LastPilot;

            foreach (var other in _lookup.GetEntitiesInRange(pos, comp.RunoverRadius))
            {
                if (other == uid)
                    continue;

                if (ignore.HasValue && other == ignore.Value)
                    continue;

                if (!TryComp<PhysicsComponent>(other, out var otherPhys))
                    continue;

                if (!TryComp<BodyComponent>(other, out var _))
                    continue;

                if (comp.LastLaunched.TryGetValue(other, out var last) && (now - last) < TimeSpan.FromSeconds(comp.ThrowCooldown))
                    continue;

                var otherPos = _xform.GetMapCoordinates(other);
                if (otherPos.MapId != pos.MapId)
                    continue;

                var toOther = otherPos.Position - pos.Position;
                if (toOther.LengthSquared() > comp.RunoverRadius * comp.RunoverRadius)
                    continue;

                if (Vector2.Dot(Vector2.Normalize(toOther), fwd) < 0.0f)
                    continue;

                var rel = physics.LinearVelocity - otherPhys.LinearVelocity;
                var relSpeed = Math.Max(rel.Length(), speed);
                var dirNorm = rel.LengthSquared() > 0.0001f ? Vector2.Normalize(rel) : fwd;
                var throwSpeed = relSpeed * comp.LaunchForceScale + comp.LaunchForceBase;

                var distance = throwSpeed * comp.AirTime;
                var dir = dirNorm * distance;

                _throwing.TryThrow(other, dir, throwSpeed, ignore, animated: true, playSound: false, throwInAir: true);
                comp.LastLaunched[other] = now;
                threwAny = true;
            }

            if (threwAny)
                _audio.PlayPvs(comp.RunOverSound, uid);
        }
    }
}
