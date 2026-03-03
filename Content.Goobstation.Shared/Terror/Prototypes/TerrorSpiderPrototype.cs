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

    [DataField]
    public HiveScalingConfig? HiveScaling;
}

[DataDefinition]
public sealed partial class HiveScalingConfig
{
    [DataField] public float Tier2BaseChance;
    [DataField] public float Tier2MaxChance;
    [DataField] public float Tier2CurveK;

    [DataField] public float Tier3BaseChance;
    [DataField] public float Tier3MaxChance;
    [DataField] public float Tier3CurveK;
}
[DataDefinition]
public sealed partial class TerrorLayConfig
{
    [DataField]
    public float Tier1Chance;

    [DataField]
    public float Tier2Chance;

    [DataField]
    public float Tier3Chance;

    [DataField]
    public float Tier4Chance;

    [DataField]
    public List<EntProtoId> Tier1 = new();

    [DataField]
    public List<EntProtoId> Tier2 = new();

    [DataField]
    public List<EntProtoId> Tier3 = new();

    [DataField]
    public List<EntProtoId> Tier4 = new(); // Likely to not ever be used unless Empress gets added one day, bless their poor soul in that case.
}
