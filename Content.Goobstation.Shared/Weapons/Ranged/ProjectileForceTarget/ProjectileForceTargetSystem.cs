using Content.Shared.Projectiles;

namespace Content.Goobstation.Shared.Weapons.Ranged.ProjectileForceTarget;

public sealed class ProjectileForceTargetSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ProjectileForceTargetComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnProjectileHit(Entity<ProjectileForceTargetComponent> ent, ref ProjectileHitEvent args) =>
        args.TargetPart = ent.Comp.Part;
}
