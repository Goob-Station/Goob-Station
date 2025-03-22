using Content.Shared._Shitmed.Surgery.Traumas;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Surgery.Wounds.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WoundComponent : Component
{
    /// <summary>
    /// 'Parent' of wound. Basically the entity to which the wound was applied.
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid HoldingWoundable;

    /// <summary>
    /// Actually, severity of the wound. The more the worse.
    /// Directly depends on <see cref="WoundSeverity"/>
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 WoundSeverityPoint;

    /// <summary>
    /// How much damage this wound does to it's parent woundable?
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly), DataField("integrityMultiplier")]
    public FixedPoint2 WoundableIntegrityMultiplier = 1;

    /// <summary>
    /// maybe some cool mechanical stuff to treat those wounds later. I genuinely have no idea
    /// Wound type. External/Internal basically.
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public WoundType WoundType = WoundType.External;

    /// <summary>
    /// Damage group of this wound.
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public string? DamageGroup;

    /// <summary>
    /// Scar wound prototype, what will be spawned upon healing this wound.
    /// If null - no scar wound will be spawned.
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public string? ScarWound;

    /// <summary>
    /// Well, name speaks for this.
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsScar;

    /// <summary>
    /// Wound severity. Has six severities: Healed/Minor/Moderate/Severe/Critical and Loss.
    /// Directly depends on <see cref="WoundSeverityPoint"/>
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public WoundSeverity WoundSeverity;

    /// <summary>
    /// When wound is visible. Always/HandScanner/AdvancedScanner.
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public WoundVisibility WoundVisibility = WoundVisibility.Always;

    /// <summary>
    /// "Can be healed".
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool CanBeHealed = true;

    /// <summary>
    /// Should this bleed?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool CanBleed = true;

    [DataField("bleedsScaling"), ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 BleedingScalingMultiplier = 1f;

    /// <summary>
    /// Should this apply trauma to the parent woundable when wound is opened / continued?
    /// </summary>
    [DataField]
    public bool CanApplyTrauma = true;

    /// <summary>
    /// Additional chance (-1, 0, 1) that is added in chance calculation
    /// </summary>
    [DataField]
    public Dictionary<TraumaType, FixedPoint2> TraumasChances = new()
    {
        { TraumaType.Dismemberment, 0 },
        { TraumaType.OrganDamage, 0 },
        { TraumaType.BoneDamage, 0 },
        { TraumaType.NerveDamage, 0 },
        { TraumaType.VeinsDamage, 0 },
    };
}
