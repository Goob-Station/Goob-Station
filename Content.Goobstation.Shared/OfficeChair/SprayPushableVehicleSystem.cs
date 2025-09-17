// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using System.Numerics;

namespace Content.Goobstation.Shared.OfficeChair;

public sealed partial class SprayPushableVehicleSystem : EntitySystem
{
    // This file is terrible code to make the velocity change feel somewhat smooth, since this is executed entirely on the server due to SpraySystem being server only.
    // I really do not care enough to make it any better. I tried doing velocity change entirely in SpraySystem and it felt like getting teleported.

    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Update(float frameTime)
    {
        var q = EntityQueryEnumerator<SprayPushableVehicleComponent, PhysicsComponent>();
        while (q.MoveNext(out var uid, out var comp, out var body))
        {
            if (comp.PendingImpulseTimeLeft <= 0f || comp.PendingImpulseRemaining == Vector2.Zero)
                continue;

            var factor = frameTime / comp.PendingImpulseTimeLeft;
            if (factor > 1f) factor = 1f;

            var dv = comp.PendingImpulseRemaining * factor;
            _physics.SetLinearVelocity(uid, body.LinearVelocity + dv);

            comp.PendingImpulseRemaining -= dv;
            comp.PendingImpulseTimeLeft -= frameTime;

            if (comp.PendingImpulseTimeLeft <= 0f || comp.PendingImpulseRemaining.LengthSquared() < 1e-8f)
            {
                comp.PendingImpulseRemaining = Vector2.Zero;
                comp.PendingImpulseTimeLeft = 0f;
            }
        }
    }

    public void EnqueueImpulse(EntityUid vehicle, Vector2 velocity)
    {
        if (!TryComp<SprayPushableVehicleComponent>(vehicle, out var comp))
            return;

        var duration = comp.ImpulseDuration <= 0f ? 0.5f : comp.ImpulseDuration;
        AddTimedImpulse(vehicle, velocity, duration);
    }

    private void AddTimedImpulse(EntityUid vehicle, Vector2 velocity, float duration)
    {
        if (!TryComp<SprayPushableVehicleComponent>(vehicle, out var comp))
            return;

        comp.PendingImpulseRemaining += velocity;
        comp.PendingImpulseTimeLeft = MathF.Max(comp.PendingImpulseTimeLeft, duration);
    }
}
