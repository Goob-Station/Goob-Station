// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Containers;

namespace Content.Shared._Hood.Weapons;

/// <summary>
/// Adds and removes <see cref="SelectiveFire.FullAuto"/> through the existing gun fire-mode implementation.
/// It does not implement firing, timing, ammunition, or projectile behavior.
/// </summary>
public sealed class SharedGunSwitchSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunSwitchCompatibleComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<GunSwitchCompatibleComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<GunSwitchCompatibleComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<GunSwitchCompatibleComponent, EntRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnComponentInit(Entity<GunSwitchCompatibleComponent> ent, ref ComponentInit args)
    {
        _itemSlots.AddItemSlot(ent.Owner, GunSwitchCompatibleComponent.SwitchSlotId, ent.Comp.SwitchSlot);
        SetAttached(ent, ent.Comp.SwitchSlot.Item is { } item && HasComp<GunSwitchComponent>(item));
    }

    private void OnComponentRemove(Entity<GunSwitchCompatibleComponent> ent, ref ComponentRemove args)
    {
        SetAttached(ent, false);
        _itemSlots.RemoveItemSlot(ent.Owner, ent.Comp.SwitchSlot);
    }

    private void OnInserted(Entity<GunSwitchCompatibleComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != GunSwitchCompatibleComponent.SwitchSlotId ||
            !HasComp<GunSwitchComponent>(args.Entity))
        {
            return;
        }

        SetAttached(ent, true);
    }

    private void OnRemoved(Entity<GunSwitchCompatibleComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID == GunSwitchCompatibleComponent.SwitchSlotId)
            SetAttached(ent, false);
    }

    private void SetAttached(Entity<GunSwitchCompatibleComponent> ent, bool attached)
    {
        if (!TryComp(ent.Owner, out GunComponent? gun))
            return;

        var baseModes = ent.Comp.BaseModes & ~SelectiveFire.FullAuto;
        if (baseModes == SelectiveFire.Invalid)
            baseModes = SelectiveFire.SemiAuto;

        var availableModes = attached
            ? baseModes | SelectiveFire.FullAuto
            : baseModes;

        if (gun.AvailableModes != availableModes)
        {
            gun.AvailableModes = availableModes;
            DirtyField(ent.Owner, gun, nameof(GunComponent.AvailableModes));
        }

        if (gun.SelectedMode is SelectiveFire.SemiAuto or SelectiveFire.Burst or SelectiveFire.FullAuto &&
            (gun.SelectedMode & availableModes) != 0)
        {
            return;
        }

        gun.SelectedMode = GetFallbackMode(availableModes);
        DirtyField(ent.Owner, gun, nameof(GunComponent.SelectedMode));
    }

    private static SelectiveFire GetFallbackMode(SelectiveFire modes)
    {
        if ((modes & SelectiveFire.SemiAuto) != 0)
            return SelectiveFire.SemiAuto;

        if ((modes & SelectiveFire.Burst) != 0)
            return SelectiveFire.Burst;

        if ((modes & SelectiveFire.FullAuto) != 0)
            return SelectiveFire.FullAuto;

        return SelectiveFire.SemiAuto;
    }
}
