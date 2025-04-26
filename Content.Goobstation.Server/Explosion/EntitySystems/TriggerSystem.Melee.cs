using Content.Server._Goobstation.Explosion.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server.Explosion.EntitySystems;

public sealed partial class GoobTriggerSystem
{
    private void InitializeMelee()
    {
        SubscribeLocalEvent<TriggerOnMeleeComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<TriggerOnMeleeComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count <= 0)
            return;

        _trigger.Trigger(ent, ent);
    }
}
