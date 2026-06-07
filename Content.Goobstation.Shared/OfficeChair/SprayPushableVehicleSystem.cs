// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using System.Numerics;
using Content.Shared.Buckle.Components;

namespace Content.Goobstation.Shared.OfficeChair;

public sealed partial class SprayPushableVehicleSystem : EntitySystem
{
    // This file is terrible code to make the velocity change feel somewhat smooth, this exists due to SpraySystem being server only.
    // I really do not care enough to make it any better. I tried doing velocity change entirely in SpraySystem and it felt like getting teleported.

    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BuckleComponent, SprayUserImpulseEvent>(OnUserSprayImpulse);
    }

    public override void Update(float frameTime)
    {
        var q = EntityQueryEnumerator<SprayPushableVehicleComponent, PhysicsComponent>();
        while (q.MoveNext(out var uid, out var comp, out var body))
        {
            if (comp.PendingImpulseTimeLeft <= TimeSpan.Zero || comp.PendingImpulseRemaining == Vector2.Zero)
                continue;

            var secsLeft = comp.PendingImpulseTimeLeft.TotalSeconds;
            var factor = secsLeft > 0 ? (float) (frameTime / secsLeft) : 1f;
            if (factor > 1f) factor = 1f;

            var dv = comp.PendingImpulseRemaining * factor;
            _physics.SetLinearVelocity(uid, body.LinearVelocity + dv);

            comp.PendingImpulseRemaining -= dv;
            comp.PendingImpulseTimeLeft -= TimeSpan.FromSeconds(frameTime);

            if (comp.PendingImpulseTimeLeft <= TimeSpan.Zero || comp.PendingImpulseRemaining.LengthSquared() < 1e-8f)
            {
                comp.PendingImpulseRemaining = Vector2.Zero;
                comp.PendingImpulseTimeLeft = TimeSpan.Zero;
            }
        }
    }

    private void OnUserSprayImpulse(Entity<BuckleComponent> ent, ref SprayUserImpulseEvent args)
    {
        var (user, buckle) = ent;
        if (!buckle.Buckled || buckle.BuckledTo is not EntityUid vehicle)
            return;

        if (!TryComp<SprayPushableVehicleComponent>(vehicle, out var pushable))
            return;

        var duration = pushable.ImpulseDuration <= 0f ? 0.5f : pushable.ImpulseDuration;
        AddTimedImpulse(vehicle, args.Impulse * pushable.Multiplier, duration);
    }

    private void AddTimedImpulse(EntityUid vehicle, Vector2 velocity, float duration)
    {
        if (!TryComp<SprayPushableVehicleComponent>(vehicle, out var comp))
            return;

        comp.PendingImpulseRemaining += velocity;

        var newWindow = TimeSpan.FromSeconds(duration);
        if (newWindow > comp.PendingImpulseTimeLeft)
            comp.PendingImpulseTimeLeft = newWindow;
    }
}
