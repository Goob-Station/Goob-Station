using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Emp;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.PowerCell;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.ModSuits;

public abstract partial class SharedModSuitSystem
{
    private void InitializeSuit()
    {
        SubscribeLocalEvent<ModSuitComponent, ComponentInit>(OnModSuitInit);
        SubscribeLocalEvent<ModSuitComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ModSuitComponent, ToggleModPartActionEvent>(OnToggleClothingAction);
        SubscribeLocalEvent<ModSuitComponent, ToggleModMenuActionEvent>(OnToggleMenuAction);
        SubscribeLocalEvent<ModSuitComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<ModSuitComponent, ComponentRemove>(OnRemoveModSuit);
        SubscribeLocalEvent<ModSuitComponent, GotEquippedEvent>(OnModSuitEquip);
        SubscribeLocalEvent<ModSuitComponent, GotUnequippedEvent>(OnModSuitUnequip);
        SubscribeLocalEvent<ModSuitComponent, ToggleModSuitPartMessage>(OnToggleClothingMessage);
        SubscribeLocalEvent<ModSuitComponent, BeingUnequippedAttemptEvent>(OnModSuitUnequipAttempt);

        SubscribeLocalEvent<ModSuitComponent, InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>>>(GetRelayedVerbs);
        SubscribeLocalEvent<ModSuitComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);
        SubscribeLocalEvent<ModSuitComponent, ToggleModPartsDoAfterEvent>(OnDoAfterComplete);
        SubscribeLocalEvent<ModSuitComponent, ModLockMessage>(OnLocked);

        SubscribeLocalEvent<ModSuitComponent, PowerCellSlotEmptyEvent>(OnPowercellEmpty);
    }

    private void OnModSuitInit(Entity<ModSuitComponent> ent, ref ComponentInit args)
    {
        ent.Comp.PartsContainer = _containerSystem.EnsureContainer<Container>(ent, ent.Comp.ContainerId);
        ent.Comp.ModuleContainer = _containerSystem.EnsureContainer<Container>(ent, ent.Comp.ModuleContainerId);
    }

    /// <summary>
    ///     On map init, either spawn the appropriate entity into the suit slot, or if it already exists, perform some
    ///     sanity checks. Also updates the action icon to show the toggled-entity.
    /// </summary>
    private void OnMapInit(Entity<ModSuitComponent> ent, ref MapInitEvent args)
    {
        if (_netMan.IsClient)
            return;

        if (ent.Comp.PartsContainer.Count != 0)
        {
            DebugTools.Assert(ent.Comp.ClothingUids.Count != 0, "Unexpected entity present inside of a modsuit container.");
            return;
        }

        StartupClothing(ent);
        StartupModules(ent);

        if (_actionContainer.EnsureAction(ent, ref ent.Comp.ActionEntity, out _, ent.Comp.Action))
            _actionsSystem.SetEntityIcon(ent.Comp.ActionEntity.Value, ent);

        _actionContainer.EnsureAction(ent, ref ent.Comp.ActionMenuEntity, ent.Comp.MenuAction);

        Dirty(ent);

        if (HasComp<PowerCellDrawComponent>(ent))
        {
            _cell.SetDrawEnabled(ent.Owner, false);
        }

        UpdateUserInterface(ent.Owner, ent.Comp);
    }

    /// <summary>
    ///     Equip or unequip the modsuit.
    /// </summary>
    private void OnToggleClothingAction(Entity<ModSuitComponent> ent, ref ToggleModPartActionEvent args)
    {
        if (!CanUseMod(ent, args.Performer, false))
            return;

        if (args.Handled)
            return;

        if (ent.Comp.UserName != null && (!_id.TryFindIdCard(args.Performer, out var id) || ent.Comp.UserName != id.Comp.FullName))
        {
            _popupSystem.PopupPredicted(Loc.GetString("modsuit-locked-popup"), args.Performer, args.Performer);
            return;
        }

        args.Handled = true;

        // If modsuit have only one attached clothing (like helmets) action will just toggle it
        // If it have more attached clothings, it'll open radial menu
        if (ent.Comp.ClothingUids.Count == 1)
            TogglePart(ent, args.Performer, GetEntity(ent.Comp.ClothingUids.First().Key));
        else
            _ui.TryToggleUi(ent.Owner, ModSuitUiKey.Key, args.Performer);
    }

    /// <summary>
    ///     Equip or unequip the modsuit.
    /// </summary>
    private void OnToggleMenuAction(Entity<ModSuitComponent> ent, ref ToggleModMenuActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        _ui.TryToggleUi(ent.Owner, ModSuitMenuUiKey.Key, args.Performer);
    }

    private void OnGetActions(Entity<ModSuitComponent> ent, ref GetItemActionsEvent args)
    {
        if (!_netMan.IsServer)
            return;

        if (!_inventorySystem.InSlotWithFlags(ent.Owner, ent.Comp.RequiredFlags))
            return;

        args.AddAction(ent.Comp.ActionMenuEntity);
        args.AddAction(ent.Comp.ActionEntity);
    }

    private void OnRemoveModSuit(Entity<ModSuitComponent> ent, ref ComponentRemove args)
    {
        // If the parent/owner component of the attached clothing is being removed (entity getting deleted?) we will
        // delete the attached entity. We do this regardless of whether or not the attached entity is currently
        // "outside" of the container or not. This means that if a hardsuit takes too much damage, the helmet will also
        // automatically be deleted.

        _actionsSystem.RemoveAction(ent.Comp.ActionEntity);

        if (_netMan.IsClient)
            return;

        foreach (var clothing in ent.Comp.ClothingUids.Keys)
        {
            QueueDel(GetEntity(clothing));
        }
    }

    /// <summary>
    ///     Called when the suit is unequipped, to ensure that the helmet also gets unequipped.
    /// </summary>
    private void OnModSuitEquip(Entity<ModSuitComponent> ent, ref GotEquippedEvent args)
    {
        ent.Comp.TempUser = args.Equipee;
    }

    /// <summary>
    ///     Called when the suit is unequipped, to ensure that the helmet also gets unequipped.
    /// </summary>
    private void OnModSuitUnequip(Entity<ModSuitComponent> ent, ref GotUnequippedEvent args)
    {
        ent.Comp.TempUser = null;

        foreach (var part in ent.Comp.ClothingUids)
        {
            // Check if entity in container what means it already unequipped
            if (ent.Comp.PartsContainer.Contains(GetEntity(part.Key)))
                continue;

            if (part.Value == null)
                continue;

            _inventorySystem.TryUnequip(args.Equipee, part.Value, force: true, predicted: true); //TODO: сделать чтобы это работало, а то сейчас писец после гиба
        }
    }

    /// <summary>
    ///     Equip or unequip modsuit with ui message
    /// </summary>
    private void OnToggleClothingMessage(Entity<ModSuitComponent> ent, ref ToggleModSuitPartMessage args)
    {
        var part = GetEntity(args.PartEntity);

        TogglePart(ent, args.Actor, part);
    }

    /// <summary>
    /// Prevents from unequipping entity if all attached not unequipped
    /// </summary>
    private void OnModSuitUnequipAttempt(Entity<ModSuitComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        if (!ent.Comp.BlockUnequipWhenAttached)
            return;

        if (GetPartsToggleStatus(ent) == ModSuitAttachedStatus.NoneToggled)
            return;

        _popupSystem.PopupPredicted(Loc.GetString("modsuit-remove-all-attached-first"), args.Unequipee, args.Unequipee);

        args.Cancel();
    }

    private void GetRelayedVerbs(Entity<ModSuitComponent> ent, ref InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>> args)
    {
        OnGetVerbs(ent, ref args.Args);
    }

    private void OnGetVerbs(Entity<ModSuitComponent> ent, ref GetVerbsEvent<EquipmentVerb> args)
    {
        if (!args.CanInteract || args.Hands == null)
            return;

        if (GetAttachedToggleCount(ent) == 0)
            return;

        if (!CanUseMod(ent, args.User))
            return;

        if (!ent.Comp.ActionEntity.HasValue)
            return;

        var text = ent.Comp.VerbText ?? Name(ent.Comp.ActionEntity.Value);

        if (args.User != ent.Comp.TempUser && ent.Comp.StripDelay == null)
            return;

        var user = args.User;

        var verb = new EquipmentVerb()
        {
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/outfit.svg.192dpi.png")),
            Text = Loc.GetString(text),
        };

        verb.Act = () => StartDoAfter(ent, user);

        args.Verbs.Add(verb);
    }

    private void OnDoAfterComplete(Entity<ModSuitComponent> ent, ref ToggleModPartsDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        ToggleClothing(ent, args.User);
    }

    private void OnLocked(EntityUid uid, ModSuitComponent comp, ModLockMessage args)
    {
        if (!comp.TempUser.HasValue)
            return;

        if (!_id.TryFindIdCard(comp.TempUser.Value, out var id))
            return;

        if (comp.UserName != null && id.Comp.FullName != comp.UserName)
            return;

        if (comp.UserName == null)
            comp.UserName = id.Comp.FullName;
        else
            comp.UserName = null;

        UpdateUserInterface(uid, comp);
    }

    private void OnPowercellEmpty(EntityUid uid, ModSuitComponent component, PowerCellSlotEmptyEvent args)
    {
        RemoveAllParts((uid, component));
    }

    private bool CanUseMod(Entity<ModSuitComponent> ent, EntityUid? user, bool checkLock = true)
    {
        if (ent.Comp.ClothingUids.Count == 0 || ent.Comp.PartsContainer == null)
            return false;

        if (ent.Comp.TempUser == null || !_inventorySystem.InSlotWithFlags(ent.Owner, ent.Comp.RequiredFlags))
            return false;

        if (checkLock && ent.Comp.UserName != null && _id.TryFindIdCard(ent.Comp.TempUser.Value, out var id) && ent.Comp.UserName != id.Comp.FullName)
            return false;

        if (HasComp<EmpDisabledComponent>(ent.Owner) && (user == ent.Comp.TempUser || user == null))
            return false;

        return true;
    }

    private bool CanToggleClothing(EntityUid user, Entity<ModSuitComponent> ent)
    {
        if (!_cell.HasDrawCharge(ent.Owner, user: user))
            return false;

        if (ent.Comp.ClothingUids.Count <= 0)
            return false;

        var ev = new ToggleClothingAttemptEvent(user, ent);
        RaiseLocalEvent(ent, ev);

        if (ev.Cancelled)
            return false;

        return true;
    }

    private void StartupClothing(Entity<ModSuitComponent> ent)
    {
        var xform = Transform(ent);

        foreach (var prototype in ent.Comp.ClothingPrototypes)
        {
            var spawned = Spawn(prototype.Value, xform.Coordinates);
            var attachedClothing = EnsureComp<ModPartComponent>(spawned);
            attachedClothing.AttachedUid = GetNetEntity(ent.Owner);

            EnsureComp<ContainerManagerComponent>(spawned);

            ent.Comp.ClothingUids.Add(GetNetEntity(spawned), prototype.Key);
            _containerSystem.Insert(spawned, ent.Comp.PartsContainer, containerXform: xform);

            Dirty(spawned, attachedClothing);
        }
    }

    private void StartupModules(Entity<ModSuitComponent> ent)
    {
        foreach (var module in ent.Comp.StartingModules)
        {
            var spawned = Spawn(module, ent.Owner.ToCoordinates());
            if (!TryComp<ModSuitModComponent>(spawned, out var moduleComp))
                continue;

            _containerSystem.Insert(spawned, ent.Comp.ModuleContainer);
            ent.Comp.CurrentComplexity += moduleComp.Complexity;

            if (moduleComp.IsInstantlyActive)
                ActivateModule(ent, (spawned, moduleComp));
        }
    }

    private void StartDoAfter(Entity<ModSuitComponent> ent, EntityUid user)
    {
        if (!ent.Comp.TempUser.HasValue)
            return;

        var wearer = ent.Comp.TempUser.Value;

        if (ent.Comp.StripDelay == null)
            return;

        var (time, stealth) = _strippable.GetStripTimeModifiers(user, wearer, ent, ent.Comp.StripDelay.Value);

        var args = new DoAfterArgs(EntityManager, user, time, new ToggleModPartsDoAfterEvent(), ent, wearer, ent)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2,
        };

        if (!_doAfter.TryStartDoAfter(args))
            return;

        if (!stealth && user != wearer)
        {
            var popup = Loc.GetString("strippable-component-alert-owner-interact", ("user", Identity.Name(user, EntityManager)), ("item", ent));
            _popupSystem.PopupEntity(popup, wearer, wearer, PopupType.Large);
        }
    }

    private void UpdateCellDraw(Entity<ModSuitComponent> ent)
    {
        if (!TryComp<PowerCellDrawComponent>(ent, out var draw))
            return;

        var attachedCount = GetAttachedToggleCount(ent);
        //_cell.QueueUpdate(ent.Owner);

        if (attachedCount <= 0)
        {
            _cell.SetDrawEnabled(ent.Owner, false);
        }
        else
        {
            _cell.SetDrawEnabled(ent.Owner, true);
            draw.DrawRate = ent.Comp.ModEnergyBaseUsing * attachedCount;
        }

    }
}
