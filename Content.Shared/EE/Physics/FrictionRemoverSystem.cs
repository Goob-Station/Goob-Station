// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.Physics;

public sealed class FrictionRemoverSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhysicsComponent, PhysicsSleepEvent>(RemoveDampening);
    }


    private void RemoveDampening(EntityUid uid, PhysicsComponent component, PhysicsSleepEvent args)
    {
        var linear = 0f;
        var angular = 0f;
        if (TryComp<PassiveDampeningComponent>(uid, out var dampening) && dampening.Enabled)
        {
            linear = dampening.LinearDampening;
            angular = dampening.AngularDampening;
        }

        // TODO: fix it
        // PhysicsSleepEvent runs on map load or something which bricks tests
        //_physics.SetAngularDamping(uid, component, angular, false);
        //_physics.SetLinearDamping(uid, component, linear);
    }
}
