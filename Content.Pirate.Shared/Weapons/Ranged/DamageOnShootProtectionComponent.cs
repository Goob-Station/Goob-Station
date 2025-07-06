using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Content.Shared.Damage;

namespace Content.Pirate.Shared.Damage.Components;


/// <summary>
/// This component is added to entities to protect them from being damaged
/// when shooting guns with the <see cref="DamageOnShootComponent"/>
/// ie. wearing a specific hardsuit can enable you to fire a Big, Powerful Gun that would normally Knock You Over.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DamageOnShootProtectionComponent : Component, IClothingSlots
{
    /// <summary>
    /// How much and what kind of damage to protect the user from
    /// when interacting with something with <see cref="DamageOnInteractComponent"/>
    /// </summary>
    [DataField(required: true)]
    public DamageModifierSet DamageProtection = default!;

    /// <summary>
    /// Only protects if the item is in the correct slot
    /// i.e. having gloves in your pocket doesn't protect you, it has to be on your hands
    /// </summary>
    [DataField]
    public SlotFlags Slots { get; set; } = SlotFlags.GLOVES;
}
