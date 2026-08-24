using Content.Server._Lavaland.Weapons.Melee.Components;
using Content.Shared._Lavaland.Weapons.Melee;
using Content.Shared.EntityEffects;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Lavaland.Weapons.Melee;

public sealed class MeleeUpgradesSystem : SharedMeleeUpgradesSystem
{
    [Dependency] private readonly SharedEntityEffectsSystem _entityEffects = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WeaponUpgradeEffectsComponent, MeleeHitEvent>(OnEffectsUpgradeHit);
    }

    private void OnEffectsUpgradeHit(Entity<WeaponUpgradeEffectsComponent> ent, ref MeleeHitEvent args)
    {
        foreach (var hit in args.HitEntities)
        {
            foreach (var effect in ent.Comp.Effects)
            {
                _entityEffects.ApplyEffect(hit, effect);
            }
        }
    }
}
