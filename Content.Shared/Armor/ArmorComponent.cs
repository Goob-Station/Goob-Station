using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

// Shitmed Change
using Content.Shared._Shitmed.Surgery.Traumas;
using Content.Shared._Shitmed.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Surgery.Traumas.Systems;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared.Armor;

/// <summary>
/// Used for clothing that reduces damage when worn.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedArmorSystem), typeof(TraumaSystem))] // Shitmed Change
public sealed partial class ArmorComponent : Component
{
    /// <summary>
    /// The damage reduction
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public DamageModifierSet Modifiers = default!;

    /// <summary>
    /// Shitmed Change: A multiplier applied to the calculated point value
    /// to determine the monetary value of the armor
    /// </summary>
    [DataField, AutoNetworkedField]
    public float PriceMultiplier = 1;

    /// <summary>
    /// Shitmed Change: If true, the coverage won't show.
    /// </summary>
    [DataField("coverageHidden")]
    public bool ArmourCoverageHidden = false;

    /// <summary>
    /// Shitmed Change: If true, the modifiers won't show.
    /// </summary>
    [DataField("modifiersHidden")]
    public bool ArmourModifiersHidden = false;

    /// <summary>
    /// Shitmed Change: thankfully all the armor in the game is symmetrical.
    /// </summary>
    [DataField("coverage")]
    public List<BodyPartType> ArmorCoverage = new();

    /// <summary>
    /// Shitmed Change: The amount of dismemberment chance deduction.
    /// </summary>
    [DataField]
    public Dictionary<TraumaType, FixedPoint2> TraumaDeductions = new()
    {
        { TraumaType.Dismemberment, 0 },
        { TraumaType.BoneDamage, 0 },
        { TraumaType.OrganDamage, 0 },
        { TraumaType.VeinsDamage, 0 },
        { TraumaType.NerveDamage, 0 },
    };
}

/// <summary>
/// Event raised on an armor entity to get additional examine text relating to its armor.
/// </summary>
/// <param name="Msg"></param>
[ByRefEvent]
public record struct ArmorExamineEvent(FormattedMessage Msg);

/// <summary>
/// A Relayed inventory event, gets the total Armor for all Inventory slots defined by the Slotflags in TargetSlots
/// </summary>
public sealed class CoefficientQueryEvent : EntityEventArgs, IInventoryRelayEvent
{
    /// <summary>
    /// All slots to relay to
    /// </summary>
    public SlotFlags TargetSlots { get; set; }

    /// <summary>
    /// The Total of all Coefficients.
    /// </summary>
    public DamageModifierSet DamageModifiers { get; set; } = new DamageModifierSet();

    public CoefficientQueryEvent(SlotFlags slots)
    {
        TargetSlots = slots;
    }
}
