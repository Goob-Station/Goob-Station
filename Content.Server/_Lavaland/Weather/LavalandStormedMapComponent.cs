using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Weather;

[RegisterComponent]
public sealed partial class LavalandStormedMapComponent : Component
{
    [DataField]
    public float Accumulator;

    [DataField]
    public ProtoId<LavalandWeatherPrototype> CurrentWeather;

    [DataField]
    public float Duration;

    [DataField]
    public float NextDamage = 1f;

    [DataField]
    public float DamageAccumulator;
}
