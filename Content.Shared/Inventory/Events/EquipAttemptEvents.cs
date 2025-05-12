// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Inventory.Events;

public abstract class EquipAttemptBase(EntityUid equipee, EntityUid equipTarget, EntityUid equipment,
    SlotDefinition slotDefinition) : CancellableEntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = SlotFlags.WITHOUT_POCKET;

    /// <summary>
    /// The entity performing the action. NOT necessarily the one actually "receiving" the equipment.
    /// </summary>
    public readonly EntityUid Equipee = equipee;

    /// <summary>
    /// The entity being equipped to.
    /// </summary>
    public readonly EntityUid EquipTarget = equipTarget;

    /// <summary>
    /// The entity to be equipped.
    /// </summary>
    public readonly EntityUid Equipment = equipment;

    /// <summary>
    /// The slotFlags of the slot to equip the entity into.
    /// </summary>
    public readonly SlotFlags SlotFlags = slotDefinition.SlotFlags;

    /// <summary>
    /// The slot the entity is being equipped to.
    /// </summary>
    public readonly string Slot = slotDefinition.Name;

    /// <summary>
    /// If cancelling and wanting to provide a custom reason, use this field. Not that this expects a loc-id.
    /// </summary>
    public string? Reason;
}

/// <summary>
/// Raised on the item that is being equipped.
/// </summary>
public sealed class BeingEquippedAttemptEvent(EntityUid equipee, EntityUid equipTarget, EntityUid equipment,
    SlotDefinition slotDefinition) : EquipAttemptBase(equipee, equipTarget, equipment, slotDefinition);

/// <summary>
/// Raised on the entity that is equipping an item.
/// </summary>
public sealed class IsEquippingAttemptEvent(EntityUid equipee, EntityUid equipTarget, EntityUid equipment,
    SlotDefinition slotDefinition) : EquipAttemptBase(equipee, equipTarget, equipment, slotDefinition);

/// <summary>
/// Raised on the entity on who item is being equipped.
/// </summary>
public sealed class IsEquippingTargetAttemptEvent(EntityUid equipee, EntityUid equipTarget, EntityUid equipment,
    SlotDefinition slotDefinition) : EquipAttemptBase(equipee, equipTarget, equipment, slotDefinition);
