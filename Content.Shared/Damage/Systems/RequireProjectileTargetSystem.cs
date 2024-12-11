using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Standing;
using Robust.Shared.Physics.Events;
using Robust.Shared.Containers;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.Damage.Components;

public sealed class RequireProjectileTargetSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RequireProjectileTargetComponent, PreventCollideEvent>(PreventCollide);
        SubscribeLocalEvent<RequireProjectileTargetComponent, StoodEvent>(StandingBulletHit);
        SubscribeLocalEvent<RequireProjectileTargetComponent, DownedEvent>(LayingBulletPass);
    }

    private void PreventCollide(Entity<RequireProjectileTargetComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.Active)
            return;

        var other = args.OtherEntity;
        // Goob edit start
        if (TryComp(other, out TargetedProjectileComponent? targeted) &&
            (targeted.Target == null || targeted.Target == ent))
            return;

        if (TryComp(other, out ProjectileComponent? projectile))
        {
            // Goob edit end

            // Prevents shooting out of while inside of crates
            var shooter = projectile.Shooter;
            if (!shooter.HasValue)
                return;

            // Goobstation - Crawling
            if (TryComp<StandingStateComponent>(shooter, out var standingState) && standingState.CurrentState != StandingState.Standing)
                return;

            if (TryComp(ent, out PhysicsComponent? physics) && physics.LinearVelocity.Length() > 2.5f) // Goobstation
                return;

            if (!_container.IsEntityOrParentInContainer(shooter.Value))
               args.Cancelled = true;
        }
    }

    private void SetActive(Entity<RequireProjectileTargetComponent> ent, bool value)
    {
        if (ent.Comp.Active == value)
            return;

        ent.Comp.Active = value;
        Dirty(ent);
    }

    private void StandingBulletHit(Entity<RequireProjectileTargetComponent> ent, ref StoodEvent args)
    {
        SetActive(ent, false);
    }

    private void LayingBulletPass(Entity<RequireProjectileTargetComponent> ent, ref DownedEvent args)
    {
        SetActive(ent, true);
    }
}
