using Content.Server._Lavaland.Procedural.Systems;
using Content.Shared.Damage;
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
    public LavalandMap Lavaland;

    [DataField]
    public DamageSpecifier Damage;

    [DataField]
    public float Duration;

    [DataField]
    public float NextDamage = 15.0f; // 15 seconds

    [DataField]
    public float DamageAccumulator;
}
