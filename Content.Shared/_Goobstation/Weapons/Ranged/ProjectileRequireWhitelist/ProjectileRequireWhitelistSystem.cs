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

    /// <summary>
    /// Handles projectile collision events based on whitelist validation.
    /// </summary>
    /// <param name="ent">The entity with the whitelist component</param>
    /// <param name="args">The collision event arguments</param>
    private void OnProjectileCollide(Entity<ProjectileRequireWhitelistComponent> ent, ref PreventCollideEvent args)
    {
        var uid = args.OtherEntity;
        var comp = ent.Comp;

        // If whitelist doesn't exist, always cancel collision
        if (comp.Whitelist == null)
        {
            args.Cancelled = true;
            return;
        }

        // Check if entity is valid against whitelist
        var isValid = _whitelist.IsValid(comp.Whitelist, uid);

        // Allow collision if (valid && !invert) OR (!valid && invert)
        if ((isValid && !comp.Invert) || (!isValid && comp.Invert))
            return;

        // Prevent collision in all other cases
        args.Cancelled = true;
    }

}
