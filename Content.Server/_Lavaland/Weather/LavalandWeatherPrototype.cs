using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Weather;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed.TypeParsers;

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
    public string PopupMessage = "You feel like wind starts blowing stronger...";

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Heat", 5 },
        },
    };
}
