// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 to4no_fix <156101927+chavonadelal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marty <martynashagriefer@gmail.com>
// SPDX-FileCopyrightText: 2025 NotActuallyMarty <martynashagriefer@gmail.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 keronshb <keronshb@live.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 to4no_fix <156101927+chavonadelal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>

// SPDX-FileCopyrightText: 2025 Marty <martynashagriefer@gmail.com>
// SPDX-FileCopyrightText: 2025 NotActuallyMarty <martynashagriefer@gmail.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Clothing;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Strip;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes;
using System.Linq;
namespace Content.Shared.Clothing.EntitySystems;

// GOOBSTATION - MODSUITS - THIS SYSTEM FULLY CHANGED
public sealed class ToggleableClothingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedStrippableSystem _strippable = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleableClothingComponent, ComponentInit>(OnToggleableInit);
        SubscribeLocalEvent<ToggleableClothingComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ToggleableClothingComponent, ToggleClothingEvent>(OnToggleClothingAction);
        SubscribeLocalEvent<ToggleableClothingComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<ToggleableClothingComponent, ComponentRemove>(OnRemoveToggleable);
        SubscribeLocalEvent<ToggleableClothingComponent, GotUnequippedEvent>(OnToggleableUnequip);
        SubscribeLocalEvent<ToggleableClothingComponent, ToggleableClothingUiMessage>(OnToggleClothingMessage);
        SubscribeLocalEvent<ToggleableClothingComponent, BeingUnequippedAttemptEvent>(OnToggleableUnequipAttempt);

        SubscribeLocalEvent<AttachedClothingComponent, ComponentInit>(OnAttachedInit);
        SubscribeLocalEvent<AttachedClothingComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<AttachedClothingComponent, GotUnequippedEvent>(OnAttachedUnequip);
        SubscribeLocalEvent<AttachedClothingComponent, ComponentRemove>(OnRemoveAttached);
        SubscribeLocalEvent<AttachedClothingComponent, BeingUnequippedAttemptEvent>(OnAttachedUnequipAttempt);

        SubscribeLocalEvent<ToggleableClothingComponent, InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>>>(GetRelayedVerbs);
        SubscribeLocalEvent<ToggleableClothingComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);
        SubscribeLocalEvent<AttachedClothingComponent, GetVerbsEvent<EquipmentVerb>>(OnGetAttachedStripVerbsEvent);
        SubscribeLocalEvent<ToggleableClothingComponent, ToggleClothingDoAfterEvent>(OnDoAfterComplete);
    }

    private void GetRelayedVerbs(Entity<ToggleableClothingComponent> toggleable, ref InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>> args)
    {
        OnGetVerbs(toggleable, ref args.Args);
    }

    private void OnGetVerbs(Entity<ToggleableClothingComponent> toggleable, ref GetVerbsEvent<EquipmentVerb> args)
    {
        var comp = toggleable.Comp;

        if (!args.CanAccess || !args.CanInteract || args.Hands == null || comp.ClothingUids.Count == 0 || comp.Container == null)
            return;

        var text = comp.VerbText ?? (comp.ActionEntity == null ? null : Name(comp.ActionEntity.Value));
        if (text == null)
            return;

        if (!_inventorySystem.InSlotWithFlags(toggleable.Owner, comp.RequiredFlags))
            return;

        var wearer = Transform(toggleable).ParentUid;
        if (args.User != wearer && comp.StripDelay == null)
            return;

        var user = args.User;

        var verb = new EquipmentVerb()
        {
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/outfit.svg.192dpi.png")),
            Text = Loc.GetString(text),
        };

        if (user == wearer)
        {
            verb.Act = () => ToggleClothing(user, toggleable);
        }
        else
        {
            verb.Act = () => StartDoAfter(user, toggleable, wearer);
        }

        args.Verbs.Add(verb);
    }

    private void StartDoAfter(EntityUid user, Entity<ToggleableClothingComponent> toggleable, EntityUid wearer)
    {
        var comp = toggleable.Comp;

        if (comp.StripDelay == null)
            return;

        var (time, stealth) = _strippable.GetStripTimeModifiers(user, wearer, toggleable, comp.StripDelay.Value);

        var args = new DoAfterArgs(EntityManager, user, time, new ToggleClothingDoAfterEvent(), toggleable, wearer, toggleable)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2,
        };

        if (!_doAfter.TryStartDoAfter(args))
            return;

        if (!stealth)
        {
            var popup = Loc.GetString("strippable-component-alert-owner-interact", ("user", Identity.Entity(user, EntityManager)), ("item", toggleable));
            _popupSystem.PopupEntity(popup, wearer, wearer, PopupType.Large);
        }
    }

    private void OnGetAttachedStripVerbsEvent(Entity<AttachedClothingComponent> attached, ref GetVerbsEvent<EquipmentVerb> args)
    {
        var comp = attached.Comp;

        if (!TryComp<ToggleableClothingComponent>(comp.AttachedUid, out var toggleableComp))
            return;

        // redirect to the attached entity.
        OnGetVerbs((comp.AttachedUid, toggleableComp), ref args);
    }

    private void OnDoAfterComplete(Entity<ToggleableClothingComponent> toggleable, ref ToggleClothingDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        ToggleClothing(args.User, toggleable);
    }

    private void OnInteractHand(Entity<AttachedClothingComponent> attached, ref InteractHandEvent args)
    {
        var comp = attached.Comp;

        if (args.Handled)
            return;

        if (!TryComp(comp.AttachedUid, out ToggleableClothingComponent? toggleableComp)
            || toggleableComp.Container == null)
            return;

        // Get slot from dictionary of uid-slot
        if (!toggleableComp.ClothingUids.TryGetValue(attached.Owner, out var attachedSlot))
            return;

        if (!_inventorySystem.TryUnequip(Transform(attached.Owner).ParentUid, attachedSlot, force: true))
            return;

        _containerSystem.Insert(attached.Owner, toggleableComp.Container);
        args.Handled = true;
    }

    /// <summary>
    /// Prevents from unequipping entity if all attached not unequipped
    /// </summary>
    private void OnToggleableUnequipAttempt(Entity<ToggleableClothingComponent> toggleable, ref BeingUnequippedAttemptEvent args)
    {
        if (!toggleable.Comp.BlockUnequipWhenAttached)
            return;

        if (GetAttachedToggleStatus(args.UnEquipTarget, toggleable, true) == ToggleableClothingAttachedStatus.NoneToggled)
            return;

        _popupSystem.PopupClient(Loc.GetString("toggleable-clothing-remove-all-attached-first"), args.Unequipee, args.Unequipee);

        args.Cancel();
    }

    /// <summary>
    ///     Called when the suit is unequipped, to ensure that the helmet also gets unequipped.
    /// </summary>
    private void OnToggleableUnequip(Entity<ToggleableClothingComponent> toggleable, ref GotUnequippedEvent args)
    {
        var comp = toggleable.Comp;

        // If it's a part of PVS departure then don't handle it.
        if (_timing.ApplyingState)
            return;

        // Check if container exists and we have linked clothings
        if (comp.Container == null || comp.ClothingUids.Count == 0)
            return;

        var parts = comp.ClothingUids;

        foreach (var part in parts)
        {
            // Check if entity in container what means it already unequipped
            if (comp.Container.Contains(part.Key))
                continue;

            if (part.Value == null)
                continue;

            _inventorySystem.TryUnequip(args.Equipee, part.Value, force: true, triggerHandContact: true);
        }
    }

    private void OnRemoveToggleable(Entity<ToggleableClothingComponent> toggleable, ref ComponentRemove args)
    {
        // If the parent/owner component of the attached clothing is being removed (entity getting deleted?) we will
        // delete the attached entity. We do this regardless of whether or not the attached entity is currently
        // "outside" of the container or not. This means that if a hardsuit takes too much damage, the helmet will also
        // automatically be deleted.

        var comp = toggleable.Comp;

        _actionsSystem.RemoveAction(comp.ActionEntity);

        if (comp.ClothingUids == null || _netMan.IsClient)
            return;

        foreach (var clothing in comp.ClothingUids.Keys)
        {
            QueueDel(clothing);
        }
    }

    private void OnAttachedUnequipAttempt(Entity<AttachedClothingComponent> attached, ref BeingUnequippedAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnRemoveAttached(Entity<AttachedClothingComponent> attached, ref ComponentRemove args)
    {
        // if the attached component is being removed (maybe entity is being deleted?) we will just remove the
        // toggleable clothing component. This means if you had a hard-suit helmet that took too much damage, you would
        // still be left with a suit that was simply missing a helmet. There is currently no way to fix a partially
        // broken suit like this.

        var comp = attached.Comp;

        if (!TryComp(comp.AttachedUid, out ToggleableClothingComponent? toggleableComp))
            return;

        if (toggleableComp.LifeStage > ComponentLifeStage.Running)
            return;

        var clothingUids = toggleableComp.ClothingUids;

        if (!clothingUids.Remove(attached.Owner))
            return;

        // If no attached clothing left - remove component and action
        if (clothingUids.Count > 0)
            return;

        _actionsSystem.RemoveAction(toggleableComp.ActionEntity);
        RemComp(comp.AttachedUid, toggleableComp);
    }

    /// <summary>
    ///     Called if the clothing was unequipped, to ensure that it gets moved into the suit's container.
    /// </summary>
    private void OnAttachedUnequip(Entity<AttachedClothingComponent> attached, ref GotUnequippedEvent args)
    {
        var comp = attached.Comp;

        // Let containers worry about it.
        if (_timing.ApplyingState)
            return;

        if (comp.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp(comp.AttachedUid, out ToggleableClothingComponent? toggleableComp))
            return;

        if (toggleableComp.LifeStage > ComponentLifeStage.Running)
            return;

        // As unequipped gets called in the middle of container removal, we cannot call a container-insert without causing issues.
        // So we delay it and process it during a system update:
        if (!toggleableComp.ClothingUids.ContainsKey(attached.Owner))
            return;

        if (toggleableComp.Container != null)
            _containerSystem.Insert(attached.Owner, toggleableComp.Container);
    }

    /// <summary>
    ///     Equip or unequip toggle clothing with ui message
    /// </summary>
    private void OnToggleClothingMessage(Entity<ToggleableClothingComponent> toggleable, ref ToggleableClothingUiMessage args)
    {
        var attachedUid = GetEntity(args.AttachedClothingUid);

        ToggleClothing(args.Actor, toggleable, attachedUid);
    }

    /// <summary>
    ///     Equip or unequip the toggleable clothing.
    /// </summary>
    private void OnToggleClothingAction(Entity<ToggleableClothingComponent> toggleable, ref ToggleClothingEvent args)
    {
        var comp = toggleable.Comp;

        if (args.Handled)
            return;

        if (comp.Container == null || comp.ClothingUids.Count == 0)
            return;

        args.Handled = true;

        // If clothing have only one attached clothing (like helmets) action will just toggle it
        // If it have more attached clothings, it'll open radial menu
        if (comp.ClothingUids.Count == 1)
            ToggleClothing(args.Performer, toggleable, comp.ClothingUids.First().Key);
        else
            _uiSystem.OpenUi(toggleable.Owner, ToggleClothingUiKey.Key, args.Performer);
    }

    /// <summary>
    ///     Attempts to equip a suit storage item to a user, showing a popup if the equip fails.
    /// </summary>
    private void ForceSuitStorage(EntityUid user, EntityUid? suitStorageItem)
    {
        if (suitStorageItem != null && !_inventorySystem.TryEquip(user, suitStorageItem.Value, "suitstorage", silent: true))
            _popupSystem.PopupClient(Loc.GetString("inventory-component-dropped-from-unequip", ("items", 1)), user, user);
    }

    /// <summary>
    ///    Finds and unequips the suit storage item from the user, returning it if found.
    /// </summary>
    private EntityUid? FindSuitStorage(EntityUid user) =>
        _inventorySystem.TryGetSlotEntity(user, "suitstorage", out var item) &&
        _inventorySystem.TryUnequip(user, "suitstorage", silent: true) ? item : null;

    /// <summary>
    ///     Toggle function for single clothing
    /// </summary>
    private void ToggleClothing(EntityUid user, Entity<ToggleableClothingComponent> toggleable, EntityUid attachedUid)
    {
        var suitStorageItem = FindSuitStorage(user);
        var comp = toggleable.Comp;
        var attachedClothings = comp.ClothingUids;
        var container = comp.Container;

        if (!CanToggleClothing(user, toggleable))
            return;

        if (!attachedClothings.TryGetValue(attachedUid, out var slot) || string.IsNullOrEmpty(slot))
            return;

        if (!container!.Contains(attachedUid))
        {
            UnequipClothing(user, toggleable, attachedUid, slot!);
            ForceSuitStorage(user, suitStorageItem);
        }
        else
        {
            EquipClothing(user, toggleable, attachedUid, slot!);
            ForceSuitStorage(user, suitStorageItem);
        }

    }

    /// <summary>
    ///     Toggle function for toggling multiple clothings at once
    /// </summary>
    private void ToggleClothing(EntityUid user, Entity<ToggleableClothingComponent> toggleable)
    {
        var comp = toggleable.Comp;
        var attachedClothings = comp.ClothingUids;
        var container = comp.Container;

        if (!CanToggleClothing(user, toggleable))
            return;

        if (GetAttachedToggleStatus(user, toggleable, false, comp) == ToggleableClothingAttachedStatus.NoneToggled)
        {
            foreach (var clothing in attachedClothings)
            {
                var suitStorageItem = FindSuitStorage(user);
                EquipClothing(user, toggleable, clothing.Key, clothing.Value);
                ForceSuitStorage(user, suitStorageItem);
            }
        }
        else
        {
            foreach (var clothing in attachedClothings)
            {
                if (!container!.Contains(clothing.Key))
                {
                    var suitStorageItem = FindSuitStorage(user);
                    UnequipClothing(user, toggleable, clothing.Key, clothing.Value);
                    ForceSuitStorage(user, suitStorageItem);
                }
            }
        }
    }

    private bool CanToggleClothing(EntityUid user, Entity<ToggleableClothingComponent> toggleable)
    {
        var comp = toggleable.Comp;
        var attachedClothings = comp.ClothingUids;
        var container = comp.Container;

        if (container == null || attachedClothings.Count == 0)
            return false;

        var ev = new ToggleClothingAttemptEvent(user, toggleable);
        RaiseLocalEvent(toggleable, ev);

        if (ev.Cancelled)
            return false;

        return true;
    }

    private void UnequipClothing(EntityUid user, Entity<ToggleableClothingComponent> toggleable, EntityUid clothing, string slot)
    {
        var parent = Transform(toggleable.Owner).ParentUid;

        _inventorySystem.TryUnequip(user, parent, slot, force: true);

        // If attached have clothing in container - equip it
        if (!TryComp<AttachedClothingComponent>(clothing, out var attachedComp) || attachedComp.ClothingContainer == null)
            return;

        var storedClothing = attachedComp.ClothingContainer.ContainedEntity;

        if (storedClothing != null)
            _inventorySystem.TryEquip(parent, storedClothing.Value, slot, force: true, triggerHandContact: true);
    }
    private void EquipClothing(EntityUid user, Entity<ToggleableClothingComponent> toggleable, EntityUid clothing, string slot)
    {
        var parent = Transform(toggleable.Owner).ParentUid;
        var comp = toggleable.Comp;

        if (_inventorySystem.TryGetSlotEntity(parent, slot, out var currentClothing))
        {
            // Check if we need to replace current clothing
            if (!TryComp<AttachedClothingComponent>(clothing, out var attachedComp) || !comp.ReplaceCurrentClothing)
            {
                _popupSystem.PopupClient(Loc.GetString("toggleable-clothing-remove-first", ("entity", currentClothing)), user, user);
                return;
            }

            // Check if attached clothing have container or this container not empty
            if (attachedComp.ClothingContainer == null || attachedComp.ClothingContainer.ContainedEntity != null)
                return;

            if (_inventorySystem.TryUnequip(user, parent, slot))
                _containerSystem.Insert(currentClothing.Value, attachedComp.ClothingContainer);
        }

        _inventorySystem.TryEquip(user, parent, clothing, slot);
    }

    private void OnGetActions(Entity<ToggleableClothingComponent> toggleable, ref GetItemActionsEvent args)
    {
        var comp = toggleable.Comp;

        if (comp.ClothingUids.Count == 0 || comp.ActionEntity == null || args.SlotFlags != comp.RequiredFlags)
            return;

        args.AddAction(comp.ActionEntity.Value);
    }

    private void OnToggleableInit(Entity<ToggleableClothingComponent> toggleable, ref ComponentInit args)
    {
        var comp = toggleable.Comp;

        comp.Container = _containerSystem.EnsureContainer<Container>(toggleable, comp.ContainerId);
    }

    private void OnAttachedInit(Entity<AttachedClothingComponent> attached, ref ComponentInit args)
    {
        var comp = attached.Comp;

        comp.ClothingContainer = _containerSystem.EnsureContainer<ContainerSlot>(attached, comp.ClothingContainerId);
    }

    /// <summary>
    ///     On map init, either spawn the appropriate entity into the suit slot, or if it already exists, perform some
    ///     sanity checks. Also updates the action icon to show the toggled-entity.
    /// </summary>
    private void OnMapInit(Entity<ToggleableClothingComponent> toggleable, ref MapInitEvent args)
    {
        var comp = toggleable.Comp;

        if (comp.Container!.Count != 0)
        {
            DebugTools.Assert(comp.ClothingUids.Count != 0, "Unexpected entity present inside of a toggleable clothing container.");
            return;
        }

        if (comp.ClothingUids.Count != 0 && comp.ActionEntity != null)
            return;

        // Add prototype from ClothingPrototype and Slot field to ClothingPrototypes dictionary
        if (comp.ClothingPrototype != null && !string.IsNullOrEmpty(comp.Slot) && !comp.ClothingPrototypes.ContainsKey(comp.Slot))
        {
            comp.ClothingPrototypes.Add(comp.Slot, comp.ClothingPrototype.Value);
        }

        var xform = Transform(toggleable.Owner);

        if (comp.ClothingPrototypes == null)
            return;

        var prototypes = comp.ClothingPrototypes;

        foreach (var prototype in prototypes)
        {
            var spawned = Spawn(prototype.Value, xform.Coordinates);
            var attachedClothing = EnsureComp<AttachedClothingComponent>(spawned);
            attachedClothing.AttachedUid = toggleable;
            EnsureComp<ContainerManagerComponent>(spawned);

            comp.ClothingUids.Add(spawned, prototype.Key);
            _containerSystem.Insert(spawned, comp.Container, containerXform: xform);

            Dirty(spawned, attachedClothing);
        }

        Dirty(toggleable, comp);

        if (_actionContainer.EnsureAction(toggleable, ref comp.ActionEntity, out var action, comp.Action))
            _actionsSystem.SetEntityIcon(comp.ActionEntity.Value, toggleable, action);
    }

    // Checks status of all attached clothings toggle status
    public ToggleableClothingAttachedStatus GetAttachedToggleStatus(EntityUid user, EntityUid toggleable, bool unequipping, ToggleableClothingComponent? component = null)
    {
        if (!Resolve(toggleable, ref component))
            return ToggleableClothingAttachedStatus.NoneToggled;

        var container = component.Container;
        var attachedClothings = component.ClothingUids;

        // If entity don't have any attached clothings it means none toggled
        if (container == null || attachedClothings.Count == 0)
            return ToggleableClothingAttachedStatus.NoneToggled;

        var toggledCount = 0;

        foreach (var attached in attachedClothings)
        {
            if (container.Contains(attached.Key)
                && unequipping
                || CheckEquipped(user, attached.Key, attached.Value) < EquipAbility.MissingSlot)
                continue;

            toggledCount++;
        }

        if (toggledCount == 0)
            return ToggleableClothingAttachedStatus.NoneToggled;

        if (toggledCount < attachedClothings.Count)
            return ToggleableClothingAttachedStatus.PartlyToggled;

        return ToggleableClothingAttachedStatus.AllToggled;
    }

    public List<EntityUid>? GetAttachedClothingsList(EntityUid toggleable, ToggleableClothingComponent? component = null)
    {
        if (!Resolve(toggleable, ref component) || component.ClothingUids.Count == 0)
            return null;

        var newList = new List<EntityUid>();

        foreach (var attachee in component.ClothingUids)
            newList.Add(attachee.Key);

        return newList;
    }

    /// <summary>
    /// Checks if an entity can equip a piece of clothing based on their body parts.
    /// </summary>
    /// <param name="equipment">The item to be equipped</param>
    /// <param name="equipTarget">The entity attempting to wear the clothing</param>
    /// <param name="slot">The slot the clothing is being equipped to</param>
    /// <returns>An enum indicating the equipment ability status</returns>
    public EquipAbility CheckEquipped(EntityUid equipTarget, EntityUid toEquip, string slot)
    {
        if (!TryComp(equipTarget, out BodyComponent? targetBody)
            || targetBody.Prototype == null
            || HasComp<BorgChassisComponent>(equipTarget))
            return EquipAbility.CannotEquip;

        // Does their species proto include the slot?
        if (!_inventorySystem.TryGetSlotContainer(equipTarget, slot, out var slotContainer, out var slotDefinition))
            return EquipAbility.MissingSlot;

        // Does the slot NOT have the item equipped?
        if (slotContainer.ContainedEntity != toEquip)
            return EquipAbility.SlotOccupiedOrEmpty;

        // Is there a body part associated with the slot?
        if (_body.TryGetPartFromSlotContainer(slot, out var bodyPart)) // If this fails, that means there's not an associated part.
        {
            var bodyPartString = bodyPart.Value.ToString().ToLower();
            var prototype = _prototypes.Index(targetBody.Prototype.Value);

            var hasPartConnection = prototype.Slots.Values.Any(protoSlot =>
                protoSlot.Connections.Contains(bodyPartString));

            // If this is a slot that requires a body part, and the part is missing
            if (hasPartConnection && !_body.GetBodyChildrenOfType(equipTarget, bodyPart.Value).Any())
                return EquipAbility.MissingPart;
        }

        // If there's no body part associated with the slot, we can equip the clothing.
        return EquipAbility.CanEquip;
    }
}

public sealed partial class ToggleClothingEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class ToggleClothingDoAfterEvent : SimpleDoAfterEvent
{
}

/// <summary>
///     Event raises on toggleable clothing when someone trying to toggle it
/// </summary>
public sealed class ToggleClothingAttemptEvent : CancellableEntityEventArgs
{
    public EntityUid User { get; }
    public EntityUid Target { get; }

    public ToggleClothingAttemptEvent(EntityUid user, EntityUid target)
    {
        User = user;
        Target = target;
    }
}

/// <summary>
/// Status of toggleable clothing attachee
/// </summary>
[Serializable, NetSerializable]
public enum ToggleableClothingAttachedStatus : byte
{
    NoneToggled,
    PartlyToggled,
    AllToggled
}
