using Content.Shared.Physics;
using Robust.Shared.Physics.Events;

namespace Content.Shared._Goobstation.Projectiles.ProjectileIgnoreTransparent;

public sealed class ProjectileIgnoreTransparentSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ProjectileIgnoreTransparentComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnPreventCollide(Entity<ProjectileIgnoreTransparentComponent> ent, ref PreventCollideEvent args)
    {
        if ((args.OtherFixture.CollisionLayer & (int) CollisionGroup.Opaque) == 0)
            args.Cancelled = true;
    }
}
