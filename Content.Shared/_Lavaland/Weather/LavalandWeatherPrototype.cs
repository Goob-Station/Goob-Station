using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Weather;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Weather;

[Prototype("lavalandWeather")]
public sealed class LavalandWeatherPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField]
    public float Duration = 150;

    [DataField]
    public float Variety = 20;

    [DataField]
    public ProtoId<WeatherPrototype> WeatherType;

    [DataField]
    public string PopupStartMessage = "You feel like wind starts blowing stronger...";

    [DataField]
    public string PopupEndMessage = "The wind is going out.";

    /// <summary>
    /// Amount of temperature to apply every tick.
    /// Be careful changing this number.
    /// </summary>
    [DataField]
    public float TemperatureChange = 30000f;

    [DataField]
    public DamageSpecifier Damage = new() {DamageDict = new Dictionary<string, FixedPoint2>() {{ "Heat", 4 }}};
}
