using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.Traits.Vampirism.Components;

/// <summary>
///     Makes a vampire (trait or antagonist) vulnerable to holy damage and suffer from holy water.
///     Shared by both vampire types so they suffer identically.
/// </summary>
[RegisterComponent]
public sealed partial class VampireHolyWaterWeaknessComponent : Component
{
    /// <summary>
    /// Delay between each holy water damage tick.
    /// </summary>
    [DataField]
    public TimeSpan HolyTickDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Reagent that counts as holy water when present in the vampire's body.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> HolyWaterReagentId = "Holywater";

    /// <summary>
    /// Burn damage applied per tick while holy water is in the body.
    /// </summary>
    [DataField]
    public float HolyWaterBurnDamage = 2f;

    /// <summary>
    /// Chance per holy water tick to be set on fire.
    /// </summary>
    [DataField]
    public float HolyWaterFireChance = 0.25f;

    /// <summary>
    /// Fire stacks applied when holy water ignites the vampire.
    /// </summary>
    [DataField]
    public float HolyWaterFireStacks = 2f;

    public TimeSpan NextHolyWaterTick = TimeSpan.Zero;

    /// <summary>
    /// Whether the entity already had a <c>WeakToHolyComponent</c> before this component was added.
    /// </summary>
    public bool HadWeakToHoly;

    /// <summary>
    /// The previous <c>AlwaysTakeHoly</c> value, restored on removal.
    /// </summary>
    public bool HadAlwaysTakeHoly;
}
