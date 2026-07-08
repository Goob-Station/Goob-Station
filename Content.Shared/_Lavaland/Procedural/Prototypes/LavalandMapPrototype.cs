// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Parallax.Biomes.Markers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared._Lavaland.Procedural.Prototypes;

/// <summary>
/// Contains information about Lavaland planet configuration.
/// </summary>
[Prototype]
public sealed partial class LavalandPlanetPrototype : IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<LavalandPlanetPrototype>))]
    public string[]? Parents { get; private set; }

    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }

    [DataField(required: true)]
    public LocId Name = "lavaland-planet-name-unknown";

    [DataField]
    public float RestrictedRange = 512f;

    [DataField(required: true)]
    public GasMixture Atmosphere = GasMixture.SpaceGas;

    [DataField]
    public float Temperature = Atmospherics.T20C;

    [DataField]
    public Color MapLight = Color.FromHex("#D8B059");

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
        "BSCrystal",
        "OreBananium",
        "OreArtifactFragment",
        "OreDiamond",
    };

    [DataField]
    public ComponentRegistry? AddComponents;
}
