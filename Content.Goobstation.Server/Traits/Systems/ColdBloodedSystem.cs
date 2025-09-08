using Content.Goobstation.Common.Traits;
using Content.Server.Temperature.Components;

namespace Content.Goobstation.Server.Traits.Systems;

public sealed class ColdBloodedSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ColdBloodedComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ColdBloodedComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnMapInit(EntityUid uid, ColdBloodedComponent component, MapInitEvent args)
    {
        // Tweak temperature damage thresholds
        EnsureComp<TemperatureComponent>(uid, out var temperature);
        temperature.ColdDamageThreshold += component.ColdThresholdIncrease;
        temperature.HeatDamageThreshold += component.HeatThresholdIncrease;
        temperature.AtmosTemperatureTransferEfficiency *= component.AtmosTransferMultiplier;
    }

    private void OnComponentRemove(EntityUid uid, ColdBloodedComponent component, ComponentRemove args)
    {
        // Revert temperature damage thresholds
        if (TryComp<TemperatureComponent>(uid, out var temperature))
        {
            temperature.ColdDamageThreshold -= component.ColdThresholdIncrease;
            temperature.HeatDamageThreshold -= component.HeatThresholdIncrease;
            temperature.AtmosTemperatureTransferEfficiency /= component.AtmosTransferMultiplier;
        }
    }
}
