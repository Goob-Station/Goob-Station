using Content.Shared._Afterlight.Silicons.Synths.Battery;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects.Body;
using Content.Shared.Power.EntitySystems;

namespace Content.Server._Afterlight.Silicons.Synths.Battery;

public sealed partial class SynthBatteryMetabolismSystem : EntitySystem
{
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private SynthBatterySystem _synthBattery = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SynthBatteryComponent, EntityEffectEvent<SatiateHunger>>(OnSatiateHunger);
    }

    private void OnSatiateHunger(Entity<SynthBatteryComponent> ent, ref EntityEffectEvent<SatiateHunger> args)
    {
        var charge = args.Effect.Factor * args.Scale * ent.Comp.NutrimentChargeMultiplier;
        if (charge <= 0f || !_synthBattery.TryGetBattery(ent.Owner, out var battery))
            return;

        _battery.ChangeCharge(battery.Value.AsNullable(), charge);
    }
}
