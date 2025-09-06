using Content.Goobstation.Common.Traits;
using Content.Server.Temperature.Components;

namespace Content.Goobstation.Server.Traits.Systems;

public sealed class ColdBloodedSystem : EntitySystem
{
    /// <summary>
    /// How much the cold damage threshold is increased when the cold-blooded trait is applied. Can be tweaked if too crippling.
    /// </summary>
    private static readonly float ColdThresholdIncrease = 7.5f; // Slightly below 20c for lizards. Below -3c for most other species.

    /// <summary>
    /// How much the heat damage threshold is increased when the cold-blooded trait is applied. Can be tweaked if too powerful.
    /// </summary>
    private static readonly float HeatThresholdIncrease = 20.0f; // 150c for lizards. 70c for most other species.

    /// <summary>
    /// How much is the ability for heat to transfer from the atmosphere to you increase. Can be tweaked if too powerful.
    /// </summary>
    private static readonly float AtmosTransferMultiplier = 2.0f;

    public override void Initialize()
    {
        SubscribeLocalEvent<ColdBloodedComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ColdBloodedComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnMapInit(EntityUid uid, ColdBloodedComponent component, MapInitEvent args)
    {
        // Tweak temperature damage thresholds
        EnsureComp<TemperatureComponent>(uid, out var temperature);
        temperature.ColdDamageThreshold += ColdThresholdIncrease;
        temperature.HeatDamageThreshold += HeatThresholdIncrease;
        temperature.AtmosTemperatureTransferEfficiency *= AtmosTransferMultiplier;
    }

    private void OnComponentRemove(EntityUid uid, ColdBloodedComponent component, ComponentRemove args)
    {
        // Revert temperature damage thresholds
        if (TryComp<TemperatureComponent>(uid, out var temperature))
        {
            temperature.ColdDamageThreshold -= ColdThresholdIncrease;
            temperature.HeatDamageThreshold -= HeatThresholdIncrease;
            temperature.AtmosTemperatureTransferEfficiency /= AtmosTransferMultiplier;
        }
    }
}
