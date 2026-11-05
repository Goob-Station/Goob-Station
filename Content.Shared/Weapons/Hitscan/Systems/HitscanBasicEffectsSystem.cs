// <Trauma>
using Content.Shared._Trauma.Projectiles;
using Robust.Shared.Network;
// </Trauma>
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Player;

namespace Content.Shared.Weapons.Hitscan.Systems;

public sealed class HitscanBasicEffectsSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!; // Trauma
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HitscanBasicEffectsComponent, HitscanDamageDealtEvent>(OnHitscanDamageDealt);
    }

    private void OnHitscanDamageDealt(Entity<HitscanBasicEffectsComponent> ent, ref HitscanDamageDealtEvent args)
    {
        if (Deleted(args.Target))
            return;

        // Trauma - not predicted so you know if it flashes its 100% a hit, same as projectiles
        if (ent.Comp.HitColor != null && args.DamageDealt.GetTotal() != 0 && _net.IsServer)
        {
            _color.RaiseEffect(ent.Comp.HitColor.Value,
                new List<EntityUid> { args.Target },
                Filter.Pvs(args.Target, entityManager: EntityManager));
        }

        _gun.PlayImpactSound(args.Target, args.DamageDealt, ent.Comp.Sound, ent.Comp.ForceSound,
            CompOrNull<PredictedShooterComponent>(ent)?.Shooter); // Trauma - predicted
    }
}
