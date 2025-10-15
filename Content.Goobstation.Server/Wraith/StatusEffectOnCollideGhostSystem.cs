using Content.Goobstation.Shared.Wraith.Collisions;
using Content.Shared.Physics;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Goobstation.Server.Wraith;

public sealed class StatusEffectOnCollideGhostSystem : SharedStatusEffectOnCollideGhostSystem
{
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    [Dependency] private readonly CollisionWakeSystem _collisionWake = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectOnCollideGhostComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StatusEffectOnCollideGhostComponent, ComponentShutdown>(OnShutdown);
    }

    private IPhysShape GetOrCreateShape(EntityUid uid, FixturesComponent? fixtures = null)
    {
        if (Resolve(uid, ref fixtures))
            if (fixtures.Fixtures.TryGetValue("fix1", out var fix))
                return fix.Shape;

        return new PhysShapeCircle(0.35f);
    }

    private void OnMapInit(Entity<StatusEffectOnCollideGhostComponent> ent, ref MapInitEvent args)
    {
        EnsureComp<PhysicsComponent>(ent.Owner);
        var fixtures = EnsureComp<FixturesComponent>(ent.Owner);
        _fixtures.TryCreateFixture(ent.Owner,
            GetOrCreateShape(ent.Owner, fixtures),
            ent.Comp.FixtureId,
            hard: false,
            collisionMask: (int)CollisionGroup.GhostImpassable,
            collisionLayer: (int)CollisionGroup.GhostImpassable,
            manager: fixtures
        );

        // Disable collision wake so that it can trigger collisions even when sitting still
        var collisionWake = EnsureComp<CollisionWakeComponent>(ent.Owner);
        _collisionWake.SetEnabled(ent.Owner, false, collisionWake);
    }

    private void OnShutdown(Entity<StatusEffectOnCollideGhostComponent> ent, ref ComponentShutdown args) =>
        _fixtures.DestroyFixture(ent.Owner, ent.Comp.FixtureId);
}
