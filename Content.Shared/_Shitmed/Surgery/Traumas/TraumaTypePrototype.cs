using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas;

/// <summary>
/// Defines a kind of trauma.
/// </summary>
[Prototype]
public sealed partial class TraumaTypePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Localization id base used for popups and the health analyzer.
    /// </summary>
    [DataField]
    public string? Name;

    /// <summary>
    /// Minimum inflicting-wound severity (delta) before this trauma is eligible to roll.
    /// </summary>
    [DataField]
    public FixedPoint2 SeverityGate = 10;

    /// <summary>
    /// Whether wounds carrying this trauma refuse to heal while it is present.
    /// </summary>
    [DataField]
    public bool BlocksHealing;

    /// <summary>
    /// Which entity in the woundable this trauma is applied to / targets.
    /// </summary>
    [DataField]
    public TraumaTarget Target = TraumaTarget.Woundable;

    /// <summary>
    /// The entity spawned to represent an instance of this trauma.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId TraumaEntity;

    /// <summary>
    /// Fallback chance (0-1) used when a cause has no <see cref="TraumaChance"/> provider of its own.
    /// </summary>
    [DataField]
    public FixedPoint2 BaseChance;

    /// <summary>
    /// Causes that can inflict this trauma (explosion, wound severity, etc.)
    /// </summary>
    [DataField]
    public List<TraumaCause>? Causes;

    /// <summary>
    /// How this trauma can be treated / removed. Null means it is not treatable on its own (e.g. it clears when the underlying bone/organ heals).
    /// </summary>
    [DataField]
    public TraumaTreatment? Treatment;

    /// <summary>
    /// Optional icon for the health analyzer / part status display.
    /// </summary>
    [DataField]
    public SpriteSpecifier? Icon;
}

/// <summary>
/// Which entity inside (or around) the woundable a trauma is applied to.
/// </summary>
[Serializable, NetSerializable]
public enum TraumaTarget : byte
{
    /// <summary>The woundable body part itself.</summary>
    Woundable,

    /// <summary>The woundable's bone entity.</summary>
    Bone,

    /// <summary>A random organ in the woundable.</summary>
    Organ,

    /// <summary>The parent woundable (used by dismemberment).</summary>
    ParentWoundable,
}
