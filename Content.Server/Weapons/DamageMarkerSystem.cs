using Content.Shared.Weapons.Marker;
// Lavaland Change
using Content.Server._Lavaland.Pressure;
using Content.Shared._Lavaland.Weapons.Marker;
using Content.Shared._White.BackStab;
using Content.Shared.Damage;
using Content.Shared.Stunnable;

namespace Content.Server.Weapons;

public sealed class DamageMarkerSystem : SharedDamageMarkerSystem
{
    // Lavaland Change Start
    [Dependency] private readonly PressureEfficiencyChangeSystem _pressure = default!;
    [Dependency] private readonly BackStabSystem _backstab = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageMarkerComponent, ApplyMarkerBonusEvent>(OnApplyMarkerBonus);
    }

    private void OnApplyMarkerBonus(EntityUid uid, DamageMarkerComponent component, ref ApplyMarkerBonusEvent args)
    {
        if (!TryComp<DamageableComponent>(uid, out var damageable))
            return;

        if (TryComp<DamageBoostOnMarkerComponent>(args.Weapon, out var boost))
        {
            var pressureMultiplier = 1f;

            if (TryComp<PressureDamageChangeComponent>(args.Weapon, out var pressure)
                && _pressure.ApplyModifier((args.Weapon, pressure)))
                pressureMultiplier = pressure.AppliedModifier;

            if (boost.BackstabBoost != null
                && _backstab.TryBackstab(uid, args.User, Angle.FromDegrees(45d), playSound: false))
                _damageable.TryChangeDamage(uid,
                (boost.BackstabBoost + boost.Boost) * pressureMultiplier,
                damageable: damageable,
                origin: args.User);
            else
                _damageable.TryChangeDamage(uid,
                boost.Boost * pressureMultiplier,
                damageable: damageable,
                origin: args.User);
        }
    }
    // Lavaland Change End
}
