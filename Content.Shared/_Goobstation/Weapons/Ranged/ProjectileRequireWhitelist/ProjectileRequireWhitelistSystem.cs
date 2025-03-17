using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Shared._Goobstation.Weapons.Ranged;

public sealed partial class ProjectileRequireWhitelistSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ProjectileRequireWhitelistComponent, PreventCollideEvent>(OnProjectileCollide);
    }

    private void OnProjectileCollide(Entity<ProjectileRequireWhitelistComponent> ent, ref PreventCollideEvent args)
    {
        var uid = args.OtherEntity;
        var comp = ent.Comp;

        // Check if the whitelist exists, and if it is valid. If both are true, return.
        if ((comp.Whitelist != null) && _whitelist.IsValid(comp.Whitelist, uid) && !comp.Invert)
            return;

        // If invert is true, and the whitelist is invalid, return.
        if ((comp.Whitelist != null) && !_whitelist.IsValid(comp.Whitelist, uid) && comp.Invert)
            return;

        // Prevent the collision
        args.Cancelled = true;
    }

}
