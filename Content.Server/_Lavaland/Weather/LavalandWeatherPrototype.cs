using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Weather;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Weather;

[Prototype]
public sealed class LavalandWeatherPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField]
    public float Duration = 300; // 5 minutes

    [DataField]
    public float Variety = 120; // +-2 minutes

    [DataField]
    public ProtoId<WeatherPrototype> WeatherType;

    [DataField]
    public string PopupStartMessage = "You feel like wind starts blowing stronger...";

    [DataField]
    public string PopupEndMessage = "The wind is going out.";

    [DataField]
    public float TemperatureChange = 20000f;
}
