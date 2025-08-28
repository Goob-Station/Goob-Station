using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Blob.Prototypes;

/// <summary>
/// Represents data needed for a blob tile.
/// </summary>
[Prototype]
public sealed partial class BlobChemPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public FixedPoint2 Cost = 70;

    /// <summary>
    /// Additional damage that is applied to the target when attacking.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Modifies how much healing all tiles receive when node pulses.
    /// </summary>
    [DataField]
    public float HealingModifier = 1f;

    /// <summary>
    /// Color that all blob tiles & some mobs will be applied with.
    /// </summary>
    [DataField]
    public Color Color = Color.White;

    [DataField]
    public List<EntityEffect> Effects = default!;
}
