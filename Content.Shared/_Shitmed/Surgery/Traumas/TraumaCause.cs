using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Body.Part;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas;

[ImplicitDataDefinitionForInheritors]
public abstract partial class TraumaCause
{
    /// <summary>
    /// If set, this cause can only inflict the trauma on these body part types. Null means any part.
    /// </summary>
    [DataField]
    public List<BodyPartType>? AllowedParts;

    /// <summary>
    /// Whether the chosen target woundable's part type is allowed by this cause.
    /// </summary>
    public bool PartAllowed(BodyPartType partType)
    {
        return AllowedParts == null || AllowedParts.Contains(partType);
    }
}

/// <summary>
/// Inflicts a trauma when the entity takes explosion damage.
/// </summary>
public sealed partial class ExplosionCause : TraumaCause
{
    /// <summary>
    /// Minimum total explosion damage dealt to this entity before a roll happens.
    /// </summary>
    [DataField]
    public FixedPoint2 MinDamage = 40;

    /// <summary>
    /// Base probability of inflicting the trauma once past <see cref="MinDamage"/>.
    /// </summary>
    [DataField]
    public float Chance = 0.25f;

    /// <summary>
    /// If true, the chance scales up as damage exceeds <see cref="MinDamage"/>.
    /// </summary>
    [DataField]
    public bool ScaleWithDamage = true;
}
