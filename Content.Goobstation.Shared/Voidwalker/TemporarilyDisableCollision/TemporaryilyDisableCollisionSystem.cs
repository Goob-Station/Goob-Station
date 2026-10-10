using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Voidwalker.TemporarilyDisableCollision;

/// <summary>
/// This component temporarily disables the entity it's attached tos collision, then re-adds it after a few seconds and removes itself.
/// Used for the Voidwalkers window ability.
/// </summary>
public sealed partial class TemporarilyDisableCollisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TemporarilyDisableCollisionComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<TemporarilyDisableCollisionComponent, ComponentRemove>(OnRemove);
    }

    private void OnInit(Entity<TemporarilyDisableCollisionComponent> entity, ref MapInitEvent args)
    {
        if (!HasComp<PhysicsComponent>(entity))
            return;

        entity.Comp.EndTime = _timing.CurTime + entity.Comp.Duration;
        _physics.SetCanCollide(entity, false);
    }

    private void OnRemove(Entity<TemporarilyDisableCollisionComponent> entity, ref ComponentRemove args)
    {
        if (!HasComp<PhysicsComponent>(entity))
            return;

        _physics.SetCanCollide(entity, true);
    }


    /// <inheritdoc />
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TemporarilyDisableCollisionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime > comp.EndTime)
                RemCompDeferred(uid, comp);
        }
    }
}
