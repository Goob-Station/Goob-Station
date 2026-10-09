using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Afterlight.Silicons.Synths;
using Content.Shared._Afterlight.Silicons.Synths.Battery;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Server._Afterlight.Silicons.Synths;

public sealed partial class SynthShockSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private SynthBatterySystem _synthBattery = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SynthShockComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<SynthShockComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased
            || args.DamageDelta == null
            || !args.DamageDelta.DamageDict.TryGetValue(ent.Comp.ShockDamageType, out var shock)
            || shock <= 0)
            return;

        if (ent.Comp.BatteryChargeMultiplier > 0f && _synthBattery.TryGetBattery(ent.Owner, out var battery))
            _battery.ChangeCharge(battery.Value.AsNullable(), shock.Float() * ent.Comp.BatteryChargeMultiplier);

        if (ent.Comp.CellularDamageMultiplier <= 0f)
            return;

        var cellular = FixedPoint2.New(shock.Float() * ent.Comp.CellularDamageMultiplier);
        if (cellular <= 0)
            return;

        var damage = new DamageSpecifier(_prototype.Index(ent.Comp.CellularDamageType), cellular);
        _damageable.TryChangeDamage(ent.Owner, damage, origin: args.Origin);
    }
}
