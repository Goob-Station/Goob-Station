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
        _physics.SetAngularDamping(uid, component, 0f, false);
        _physics.SetLinearDamping(uid, component, 0f);
    }
}
