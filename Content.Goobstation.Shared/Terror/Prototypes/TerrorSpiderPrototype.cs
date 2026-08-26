using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Prototypes;

/// <summary>
/// For specifying what each Terror Spider can do.
/// </summary>
[Prototype]
public sealed partial class TerrorSpiderPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// What tier of spider this prototype is.
    /// </summary>
    [DataField]
    public int Tier = 1;

    /// <summary>
    /// If this is the Queen. We use this instead of just relying on tiers to inform the systems so nothing breaks.
    /// </summary>
    [DataField]
    public bool IsQueen = false;

    /// <summary>
    /// If can lay eggs. Duh.
    /// </summary>
    [DataField]
    public bool IsEggLayer = false;

    /// <summary>
    /// If should become invisible while walking on webs.
    /// </summary>
    [DataField]
    public bool IsInvisibleOnWeb = false;

    /// <summary>
    /// If should infest with spiderlings on hitting targets.
    /// </summary>
    [DataField]
    public bool CanInfestOnHit = false;

    /// <summary>
    /// If this spider can wrap corpses. Theoretically true for every Terror Spider, but just in case.
    /// </summary>
    [DataField]
    public bool CanWrap = true;

    [DataField]
    public SoundSpecifier? DeathSound;

    /// <summary>
    /// The mob entity prototype to spawn when a spiderling matures into this variant.
    /// </summary>
    [DataField]
    public EntProtoId? MobPrototype;

    [DataField]
    public TerrorLayConfig? LayConfig;
}

[DataDefinition]
public sealed partial class TerrorLayConfig
{
    [DataField(required: true)]
    public List<TerrorLayTierChance> Tiers = new();
}

[DataDefinition]
public sealed partial class TerrorLayTierChance
{
    [DataField(required: true)]
    public int Tier;

    [DataField(required: true)]
    public List<EntProtoId> Eggs = new();

    [DataField(required: true)]
    public float BaseChance;

    [DataField]
    public float? MaxChance;

    /// <summary>
    /// Exponential curve for scaling.
    /// Lower means faster scaling, higher means slower.
    /// </summary>
    [DataField]
    public float? CurveK;

    [DataField]
    public bool ScaleWithHive = false;
}
