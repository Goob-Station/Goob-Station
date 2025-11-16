using System.Linq;
using Content.Shared.Interaction;
using Content.Shared.Inventory.Events;
using Content.Shared.Verbs;
using Content.Shared.Wires;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.ModSuits;

public abstract partial class SharedModSuitSystem
{
    private void InitializeParts()
    {
        SubscribeLocalEvent<ModPartComponent, ComponentInit>(OnAttachedInit);
        SubscribeLocalEvent<ModPartComponent, ComponentRemove>(OnRemoveAttached);

        SubscribeLocalEvent<ModPartComponent, GotUnequippedEvent>(OnAttachedUnequip);
        SubscribeLocalEvent<ModPartComponent, BeingUnequippedAttemptEvent>(OnAttachedUnequipAttempt);

        SubscribeLocalEvent<ModPartComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<ModPartComponent, GetVerbsEvent<EquipmentVerb>>(OnGetAttachedStripVerbsEvent);
    }

    private void OnAttachedInit(Entity<ModPartComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ClothingContainer = _containerSystem.EnsureContainer<ContainerSlot>(ent, ent.Comp.ClothingContainerId);
    }

    private void OnRemoveAttached(Entity<ModPartComponent> ent, ref ComponentRemove args)
    {
        // if the attached component is being removed (maybe entity is being deleted?) we will just remove the
        // modsuit component. This means if you had a hard-suit helmet that took too much damage, you would
        // still be left with a suit that was simply missing a helmet. There is currently no way to fix a partially
        // broken suit like this.

        var suit = GetEntity(ent.Comp.AttachedUid);

        if (!TryComp(suit, out ModSuitComponent? modSuitComp))
            return;

        if (!modSuitComp.ClothingUids.Remove(GetNetEntity(ent.Owner)))
            return;

        // If no attached clothing left - remove component and action
        if (modSuitComp.ClothingUids.Count > 0)
            return;

        _actionsSystem.RemoveAction(modSuitComp.ActionEntity);
        RemComp(suit, modSuitComp);
    }

    /// <summary>
    ///     Called if the clothing was unequipped, to ensure that it gets moved into the suit's container.
    /// </summary>
    private void OnAttachedUnequip(Entity<ModPartComponent> ent, ref GotUnequippedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        var suit = GetEntity(ent.Comp.AttachedUid);

        if (!TryComp(suit, out ModSuitComponent? modSuitComp))
            return;

        // As unequipped gets called in the middle of container removal, we cannot call a container-insert without causing issues.
        // So we delay it and process it during a system update:
        if (!modSuitComp.ClothingUids.ContainsKey(GetNetEntity(ent.Owner)))
            return;

        _containerSystem.Insert(ent.Owner, modSuitComp.PartsContainer);
    }

    private void OnAttachedUnequipAttempt(Entity<ModPartComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnInteractHand(Entity<ModPartComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (_timing.ApplyingState)
            return;

        var suit = GetEntity(ent.Comp.AttachedUid);

        if (!TryComp(suit, out ModSuitComponent? modSuitComp) || !modSuitComp.TempUser.HasValue)
            return;

        // Get slot from dictionary of uid-slot
        if (!modSuitComp.ClothingUids.TryGetValue(GetNetEntity(ent.Owner), out var attachedSlot))
            return;

        if (!_inventorySystem.TryUnequip(modSuitComp.TempUser.Value, attachedSlot, force: true, predicted: true))
            return;

        _containerSystem.Insert(ent.Owner, modSuitComp.PartsContainer);
        args.Handled = true;
    }

    private void OnGetAttachedStripVerbsEvent(Entity<ModPartComponent> ent, ref GetVerbsEvent<EquipmentVerb> args)
    {
        var suit = GetEntity(ent.Comp.AttachedUid);

        if (!TryComp<ModSuitComponent>(suit, out var modSuitComp))
            return;

        // redirect to the attached entity.
        OnGetVerbs((suit, modSuitComp), ref args);
    }

    /// <summary>
    ///     Toggle function for toggling multiple clothings at once
    /// </summary>
    private void ToggleClothing(Entity<ModSuitComponent> ent, EntityUid user)
    {
        if (!CanToggleClothing(user, ent))
            return;

        if (GetPartsToggleStatus(ent, ent.Comp) == ModSuitAttachedStatus.NoneToggled)
        {
            foreach (var clothing in ent.Comp.ClothingUids)
                EquipPart(ent, user, GetEntity(clothing.Key), clothing.Value, false);
        }
        else
        {
            foreach (var clothing in ent.Comp.ClothingUids.Where(x => !ent.Comp.PartsContainer.Contains(GetEntity(x.Key))))
                UnequipPart(ent, user, GetEntity(clothing.Key), clothing.Value, false);
        }

        UpdateUserInterface(ent, ent.Comp);
    }

    /// <summary>
    ///     Toggle function for single clothing
    /// </summary>
    private void TogglePart(Entity<ModSuitComponent> ent, EntityUid user, EntityUid part)
    {
        if (!TryComp<WiresPanelComponent>(ent, out var panel) || panel.Open)
        {
            _popupSystem.PopupPredicted(Loc.GetString("modsuit-close-wires"), user, user);
            return;
        }

        if (!CanToggleClothing(user, ent))
            return;

        if (!ent.Comp.ClothingUids.TryGetValue(GetNetEntity(part), out var slot) || string.IsNullOrEmpty(slot))
            return;

        if (!ent.Comp.PartsContainer.Contains(part))
            UnequipPart(ent, user, part, slot);
        else
            EquipPart(ent, user, part, slot);
    }

    private void EquipPart(Entity<ModSuitComponent> ent, EntityUid user, EntityUid part, string slot, bool updateUi = true)
    {
        if (_timing.ApplyingState)
            return;

        if (!ent.Comp.TempUser.HasValue)
            return;

        if (!TryComp<ModPartComponent>(part, out var attachedComp))
            return;

        var parent = ent.Comp.TempUser.Value;
        _inventorySystem.TryGetSlotEntity(parent, slot, out var currentClothing);

        // Check if we need to replace current clothing
        if (currentClothing.HasValue && !ent.Comp.ReplaceCurrentClothing)
        {
            _popupSystem.PopupPredicted(Loc.GetString("modsuit-remove-first", ("entity", currentClothing)), user, user);
            return;
        }

        if (_inventorySystem.TryUnequip(user, parent, slot, out var removedItem, predicted: true))
            _containerSystem.Insert(removedItem.Value, attachedComp.ClothingContainer);
        else if (removedItem.HasValue)
            return;

        _inventorySystem.TryEquip(user, parent, part, slot, force: true, predicted: true);

        UpdateCellDraw(ent);

        if (GetPartsToggleStatus(ent.Owner, ent.Comp) == ModSuitAttachedStatus.AllToggled && _timing.IsFirstTimePredicted && _netMan.IsClient)
            _audio.PlayGlobal(ent.Comp.FullyEnabledSound, user);

        if (updateUi)
            UpdateUserInterface(ent.Owner, ent.Comp);
    }

    private void UnequipPart(Entity<ModSuitComponent> ent, EntityUid user, EntityUid clothing, string slot, bool updateUi = true)
    {
        if (_timing.ApplyingState)
            return;

        if (!ent.Comp.TempUser.HasValue)
            return;

        var parent = ent.Comp.TempUser.Value;

        _inventorySystem.TryUnequip(user, parent, slot, force: true, predicted: true);

        // If attached have clothing in container - equip it
        if (!TryComp<ModPartComponent>(clothing, out var attachedComp))
            return;

        if (attachedComp.ClothingContainer.ContainedEntity is { Valid: true } stored)
            _inventorySystem.TryEquip(parent, stored, slot, force: true, predicted: true);

        UpdateCellDraw(ent);

        if (updateUi)
            UpdateUserInterface(ent.Owner, ent.Comp);
    }

    private void RemoveAllParts(Entity<ModSuitComponent> ent)
    {
        foreach (var clothing in ent.Comp.ClothingUids)
        {
            if (!ent.Comp.PartsContainer.Contains(GetEntity(clothing.Key)))
                UnequipPart(ent, ent, GetEntity(clothing.Key), clothing.Value, false);
        }

        UpdateUserInterface(ent, ent.Comp);
    }

    public int GetAttachedToggleCount(Entity<ModSuitComponent> ent)
        => ent.Comp.ClothingUids.Where(x => !ent.Comp.PartsContainer.Contains(GetEntity(x.Key))).Count();

    public ModSuitAttachedStatus GetPartsToggleStatus(EntityUid modSuit, ModSuitComponent? component = null)
    {
        if (!Resolve(modSuit, ref component))
            return ModSuitAttachedStatus.NoneToggled;

        // If entity don't have any attached clothings it means none toggled
        if (component.ClothingUids.Count == 0)
            return ModSuitAttachedStatus.NoneToggled;

        var toggledCount = component.ClothingUids.Where(x => !component.PartsContainer.Contains(GetEntity(x.Key))).Count();

        if (toggledCount <= 0)
            return ModSuitAttachedStatus.NoneToggled;

        if (toggledCount < component.ClothingUids.Count)
            return ModSuitAttachedStatus.PartlyToggled;

        return ModSuitAttachedStatus.AllToggled;
    }
}
