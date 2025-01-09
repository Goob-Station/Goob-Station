using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Penalties.Components;
/// <summary>
/// This will apply a movespeed multiplier and damage on an entity when outerlayer item is equipped
/// Additionally, length of the doafters for equipping and unequipping these will be increased
/// </summary>

[RegisterComponent, NetworkedComponent]
public sealed partial class OuterSlotPenaltyComponent : Component
{
    /// <summary>
    /// Movement speed multiplier, applied when worn only to prevent slowdown while in hand
    /// </summary>
    [DataField(required: true)]
    public float EquippedSpeedMultiplier = 1.0f;

    /// <summary>
    /// Current state of outerlayer inventory slot
    /// </summary>
    [DataField]
    public bool OuterLayerEquipped = false;

    /// <summary>
    /// Damage dealt to owner on successful outerlayer equip attempt
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Equip delay applied to outerlayer cloth when owner has it in inventory/equipped
    /// It's a flat value, since most equips in this slot are instantenous, so mult wouldn't make sense
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public float EquipDelay = 2f;

    /// <summary>
    /// Unequip delay applied to outerlayer cloth when owner has it in inventory/equipped
    /// It's a flat value, since most unequips in this slot are instantenous, so mult wouldn't make sense
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public float UnequipDelay = 2f;
}
