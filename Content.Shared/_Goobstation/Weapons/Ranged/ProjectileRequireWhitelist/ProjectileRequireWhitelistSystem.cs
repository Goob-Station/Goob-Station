using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Shared._Goobstation.Weapons.Ranged;

public sealed partial class ProjectileRequireWhitelistSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ProjectileRequireWhitelistComponent, StartCollideEvent>(OnProjectileCollide);
    }

    private void OnProjectileCollide(Entity<ProjectileRequireWhitelistComponent> ent, ref StartCollideEvent args)
    {
        var projectile = args.OurEntity;
        var uid = args.OtherEntity;
        var comp = ent.Comp;

        // Check if the whitelist exists, and if it is valid. If both are true, return.
        if ((comp.Whitelist != null) && _whitelist.IsValid(comp.Whitelist, uid) && !comp.Invert)
            return;

        // If invert is true, and the whitelist is invalid, return.
        if ((comp.Whitelist != null) && !_whitelist.IsValid(comp.Whitelist, uid) && comp.Invert)
            return;

        // If the whitelist is invalid, delete the projectile.
        EntityManager.DeleteEntity(projectile);
    }

}
