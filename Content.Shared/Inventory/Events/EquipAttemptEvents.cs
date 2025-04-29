// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Inventory.Events;

public abstract class EquipAttemptBase : CancellableEntityEventArgs
{
    /// <summary>
    /// The entity performing the action. NOT necessarily the one actually "receiving" the equipment.
    /// </summary>
    public readonly EntityUid Equipee;

    /// <summary>
    /// The entity being equipped to.
    /// </summary>
    public readonly EntityUid EquipTarget;

    /// <summary>
    /// The entity to be equipped.
    /// </summary>
    public readonly EntityUid Equipment;

    /// <summary>
    /// The slotFlags of the slot to equip the entity into.
    /// </summary>
    public readonly SlotFlags SlotFlags;

    /// <summary>
    /// The slot the entity is being equipped to.
    /// </summary>
    public readonly string Slot;

    /// <summary>
    /// If cancelling and wanting to provide a custom reason, use this field. Not that this expects a loc-id.
    /// </summary>
    public string? Reason;

    public EquipAttemptBase(EntityUid equipee, EntityUid equipTarget, EntityUid equipment,
        SlotDefinition slotDefinition)
    {
        EquipTarget = equipTarget;
        Equipment = equipment;
        Equipee = equipee;
        SlotFlags = slotDefinition.SlotFlags;
        Slot = slotDefinition.Name;
    }
}

public sealed class BeingEquippedAttemptEvent : EquipAttemptBase
{
    public BeingEquippedAttemptEvent(EntityUid equipee, EntityUid equipTarget, EntityUid equipment,
        SlotDefinition slotDefinition) : base(equipee, equipTarget, equipment, slotDefinition)
    {
    }
}

public sealed class IsEquippingAttemptEvent : EquipAttemptBase
{
    public IsEquippingAttemptEvent(EntityUid equipee, EntityUid equipTarget, EntityUid equipment,
        SlotDefinition slotDefinition) : base(equipee, equipTarget, equipment, slotDefinition)
    {
    }
}