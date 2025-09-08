using Content.Goobstation.Common.Traits;
using Content.Server.Temperature.Components;

namespace Content.Goobstation.Server.Traits.Systems;

public sealed class ColdBloodedSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ColdBloodedComponent, MapInitEvent>(OnMapInit); // SHITCODE!!!!!!!
        SubscribeLocalEvent<ColdBloodedComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnMapInit(Entity<ColdBloodedComponent> ent, ref MapInitEvent args)
    {
        // Tweak temperature damage thresholds
        EnsureComp<TemperatureComponent>(ent, out var temperature);
        temperature.ColdDamageThreshold += ent.Comp.ColdThresholdIncrease;
        temperature.HeatDamageThreshold += ent.Comp.HeatThresholdIncrease;
        temperature.AtmosTemperatureTransferEfficiency *= ent.Comp.AtmosTransferMultiplier;
    }

    private void OnComponentRemove(Entity<ColdBloodedComponent> ent, ref ComponentRemove args)
    {
        // Revert temperature damage thresholds
        if (TryComp<TemperatureComponent>(ent, out var temperature))
        {
            temperature.ColdDamageThreshold -= ent.Comp.ColdThresholdIncrease;
            temperature.HeatDamageThreshold -= ent.Comp.HeatThresholdIncrease;
            temperature.AtmosTemperatureTransferEfficiency /= ent.Comp.AtmosTransferMultiplier;
        }
    }
}
