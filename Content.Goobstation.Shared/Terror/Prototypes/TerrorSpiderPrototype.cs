using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Prototypes;

[Prototype("terrorSpider")]
public sealed class TerrorSpiderPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public int Tier = 1;

    [DataField]
    public bool CanLay = false;

    [DataField]
    public bool IsInvisibleOnWeb = false;

    [DataField]
    public TerrorLayConfig? LayConfig;

    [DataField]
    public bool InfestsOnHit = false;
}

[DataDefinition]
public sealed partial class TerrorLayConfig
{
    [DataField(required: true)]
    public List<TerrorLayTier> Tiers = new();
}

[DataDefinition]
public sealed partial class TerrorLayTier
{
    /// <summary>
    /// The eggs this tier can spawn.
    /// </summary>
    [DataField(required: true)]
    public List<EntProtoId> Prototypes = new();

    /// <summary>
    /// Base chance for the tier.
    /// </summary>
    [DataField(required: true)]
    public float BaseChance;

    /// <summary>
    /// Optional scaling for the egg tiers.
    /// </summary>
    [DataField]
    public float? MaxChance;

    [DataField]
    public float? CurveK;

    /// <summary>
    /// Requires Queen for scaling.
    /// </summary>
    [DataField]
    public bool ScaleWithHive = false;
}
