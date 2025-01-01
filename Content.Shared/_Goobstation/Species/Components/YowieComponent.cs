using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Species.Components;
/// <summary>
/// This will apply a movespeed multiplier and damage on an entity when outerlayer item is worn
/// </summary>

[RegisterComponent, NetworkedComponent]
public sealed partial class YowieComponent : Component
{
    /// <summary>
    /// Movement speed multiplier, applied when worn
    /// </summary>
    [DataField(required: true)]
    public float SoftSuitSpeedMultiplier = default!;

    /// <summary>
    /// Current state of outerlayer inventory slot
    /// </summary>
    [DataField]
    public bool OuterLayerEquipped = false;

    /// <summary>
    /// Damage dealt to owner on succesful outerlayer equip attempt
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    /// <summary>
    /// Equip delay applied to outerlayer cloth when owner has it
    /// </summary>
    [DataField]
    public float EquipDelay = 2f;

    /// <summary>
    /// Unequip delay applied to outerlayer cloth when owner has it
    /// </summary>
    [DataField]
    public float UnequipDelay = 2f;
}
