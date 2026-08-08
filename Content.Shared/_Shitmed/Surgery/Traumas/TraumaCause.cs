using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Body.Part;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

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

/// <summary>
/// Inflicts a trauma when a wound of a matching damage type gains severity.
public sealed partial class WoundSeverityCause : TraumaCause
{
    /// <summary>
    /// Damage types of wound that can inflict this trauma. Null/empty means any damaging wound.
    /// </summary>
    [DataField]
    public List<ProtoId<DamageTypePrototype>>? DamageTypes;

    /// <summary>
    /// Formula that computes the base chance for this cause. Null uses the prototype's BaseChance.
    /// </summary>
    [DataField]
    public TraumaChance? Chance;

    /// <summary>
    /// Whether a wound of the given damage type can inflict this trauma.
    /// </summary>
    public bool DamageAllowed(ProtoId<DamageTypePrototype> damageType)
    {
        return DamageTypes == null || DamageTypes.Count == 0 || DamageTypes.Contains(damageType);
    }
}
