using Content.Server.Body.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Projectiles;
using Content.Shared.Gibbing.Components;

namespace Content.Goobstation.Server.Weapons;

/// <summary>
/// Granted to a mob, or item (melee or projectile) which grants the ability to instantly gib an attacked entity upon hit.
/// </summary>
public sealed class GibMobOnHitSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _bodySystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GibMobOnHitComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<GibMobOnHitComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    public void OnMeleeHit(Entity<GibMobOnHitComponent> ent, ref MeleeHitEvent args)
    {
        foreach (var hitEntity in args.HitEntities)
        {
            TryToGibEntity(hitEntity);
        }
    }

    public void OnProjectileHit(Entity<GibMobOnHitComponent> ent, ref ProjectileHitEvent args) 
    {
        var hitEntity = args.Target;

        TryToGibEntity(hitEntity);
    }

    public void TryToGibEntity(EntityUid hitEntity)
    {
        if (HasComp<GibbableComponent>(hitEntity))
        {
            if (!HasComp<GodmodeComponent>(hitEntity))
                _bodySystem.GibBody(hitEntity);
        }
    }
}
