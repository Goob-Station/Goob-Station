using Content.Shared._Lavaland.Weather;
using Content.Shared.Atmos;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Parallax.Biomes.Markers;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Procedural.Prototypes;

/// <summary>
/// Contains information about Lavaland planet configuration.
/// </summary>
[Prototype]
public sealed partial class LavalandMapPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField] public LocId Name = "lavaland-planet-name-unknown";

    [DataField]
    public string OutpostPath = "";

    [DataField]
    public float RestrictedRange = 512f;

    [DataField(required: true)]
    public ProtoId<LavalandRuinPoolPrototype> RuinPool;

    [DataField(required: true)]
    public EntityWhitelist ShuttleWhitelist = new();

    #region Atmos

    [DataField]
    public float[] Atmosphere = new float[Atmospherics.AdjustedNumberOfGases];

    [DataField]
    public float Temperature = Atmospherics.T20C;

    [DataField]
    public Color? PlanetColor;

    #endregion

    #region Biomes

    [DataField("biome", required: true)]
    public ProtoId<BiomeTemplatePrototype> BiomePrototype;

    [DataField("markers")]
    public List<ProtoId<BiomeMarkerLayerPrototype>> OreLayers = new()
    {
        "OreIron",
        "OreCoal",
        "OreQuartz",
        "OreGold",
        "OreSilver",
        "OrePlasma",
        "OreUranium",
        "OreBananium",
        "OreArtifactFragment",
        "OreDiamond",
        "BSCrystal",
    };

    [DataField("weather")]
    public List<ProtoId<LavalandWeatherPrototype>>? AvailableWeather;

    #endregion
}
