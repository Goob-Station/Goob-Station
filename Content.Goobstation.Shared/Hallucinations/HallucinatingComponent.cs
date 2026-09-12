using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hallucinations;

/// <summary>
/// While this is present the entity starts hallucinating. Hallucinations include fake mobs, fake clothes / items on players, and fake sounds.
/// </summary>
[RegisterComponent]
public sealed partial class HallucinatingComponent : Component
{
    /// <summary>
    /// Weighted table of hallucination groups.
    /// </summary>
    [DataField]
    public ProtoId<WeightedRandomPrototype> Groups = "HallucinationGroups";

    /// <summary>
    /// Scales how often hallucinations fire. the delay between them is divided by this,
    /// so 2 means twice as often (15-60s at base delays), 0.5 means half as often (60-240s).
    /// At the default 1 the base 30-120s range applies.
    /// </summary>
    [DataField]
    public float Severity = 1f;

    /// <summary>
    /// Base delay between hallucinations.
    /// </summary>
    [DataField]
    public float DelayMin = 30f;

    [DataField]
    public float DelayMax = 120f;

    [ViewVariables]
    public TimeSpan NextTime;

    [DataField]
    public EntityUid? CurrentVictim;
}
