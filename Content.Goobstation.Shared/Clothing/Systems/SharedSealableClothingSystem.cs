// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marty <martynashagriefer@gmail.com>
// SPDX-FileCopyrightText: 2025 Martynas6ha4 <martynashagriefer@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Popups;
using Content.Shared.PowerCell;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Robust.Shared.Timing;
using Vector2 = System.Numerics.Vector2;
using Content.Shared.Silicons.StationAi;


namespace Content.Goobstation.Shared.Clothing.Systems;

/// <summary>
///     System used for sealable clothing (like modsuits)
/// </summary>
public abstract class SharedSealableClothingSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainerSystem = default!;
    [Dependency] private readonly ComponentTogglerSystem _componentTogglerSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPowerCellSystem _powerCellSystem = default!;
    [Dependency] private readonly ToggleableClothingSystem _toggleableSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SealableClothingComponent, ClothingPartSealCompleteEvent>(OnPartSealingComplete);

        SubscribeLocalEvent<SealableClothingControlComponent, ClothingControlSealCompleteEvent>(OnControlSealingComplete);
        SubscribeLocalEvent<SealableClothingControlComponent, ClothingGotEquippedEvent>(OnControlEquip);
        SubscribeLocalEvent<SealableClothingControlComponent, ClothingGotUnequippedEvent>(OnControlUnequip);
        SubscribeLocalEvent<SealableClothingControlComponent, ComponentRemove>(OnControlRemove);
        SubscribeLocalEvent<SealableClothingControlComponent, GetItemActionsEvent>(OnControlGetItemActions);
        SubscribeLocalEvent<SealableClothingControlComponent, GetVerbsEvent<EquipmentVerb>>(OnEquipmentVerb);
        SubscribeLocalEvent<SealableClothingControlComponent, MapInitEvent>(OnControlMapInit);
        SubscribeLocalEvent<SealableClothingControlComponent, SealClothingDoAfterEvent>(OnSealClothingDoAfter);
        SubscribeLocalEvent<SealableClothingControlComponent, SealClothingEvent>(OnControlSealEvent);
        SubscribeLocalEvent<SealableClothingControlComponent, StartSealingProcessDoAfterEvent>(OnStartSealingDoAfter);
        SubscribeLocalEvent<SealableClothingControlComponent, ToggleClothingAttemptEvent>(OnToggleClothingAttempt);
        SubscribeLocalEvent<SealableClothingControlComponent, ToggledBackClothingFullUnequipAndInsertedEvent>(OnBackClothingUnequipped);
        SubscribeLocalEvent<SealableClothingControlComponent, BeingUnequippedAttemptEvent>(OnToggleableUnequipAttemptSealCheck);
        SubscribeLocalEvent<SealableClothingControlComponent, OnToggleableUnequipAttemptEvent>(OnToggleSanityChecker);
        SubscribeLocalEvent<SealableClothingControlComponent, InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>>>(OnRelayedVerbRequest);

        SubscribeLocalEvent<SealableClothingComponent, OnAttachedUnequipAttemptEvent>(OnAttachedUnequipAttemptSealCheck);



    }

    #region Events

    /// <summary>
    /// Toggles components on part when suit complete sealing process
    /// </summary>
    /// <param name="part"></param>
    /// <param name="args"></param>
    private void OnPartSealingComplete(Entity<SealableClothingComponent> part, ref ClothingPartSealCompleteEvent args)
    {
        _componentTogglerSystem.ToggleComponent(part, args.IsSealed);
    }

    /// <summary>
    ///     Toggles components on control when suit complete sealing process
    /// </summary>
    private void OnControlSealingComplete(Entity<SealableClothingControlComponent> control, ref ClothingControlSealCompleteEvent args)
    {
        if (control.Comp.WearerEntity == null)
            return;

        _componentTogglerSystem.ToggleComponent(control, args.IsSealed);

        // I need to untoggleclothing but i cba to move this to goobmod common so im just forceunequipping the suit after unseal
        if (!control.Comp.UnequipAfterUnseal)
            return;
        if (args.IsSealed)
        {
            control.Comp.UnequipAfterUnseal = false;
            return;
        }

        var slot = control.Comp.RequiredControlSlot.ToString().ToLowerInvariant();
        var wearer = control.Comp.WearerEntity;
        _inventorySystem.TryUnequip(wearer.Value, wearer.Value, slot, force:true);
        _inventorySystem.TryEquip(wearer.Value, wearer.Value, control, slot, force: true);
        control.Comp.UnequipAfterUnseal = false;
    }

    /// <summary>
    /// Add/Remove wearer on clothing equip/unequip
    /// </summary>
    private void OnControlEquip(Entity<SealableClothingControlComponent> control, ref ClothingGotEquippedEvent args)
    {
        control.Comp.WearerEntity = args.Wearer;
        Dirty(control);
    }

    private void OnControlUnequip(Entity<SealableClothingControlComponent> control, ref ClothingGotUnequippedEvent args)
    {
        control.Comp.WearerEntity = null;
        Dirty(control);
    }

    /// <summary>
    /// Removes seal action on component remove
    /// </summary>
    private void OnControlRemove(Entity<SealableClothingControlComponent> control, ref ComponentRemove args)
    {
        var comp = control.Comp;

        _actionsSystem.RemoveAction(comp.SealActionEntity);
    }

    /// <summary>
    /// Ensures seal action to wearer when it equip the seal control
    /// </summary>
    private static void OnControlGetItemActions(Entity<SealableClothingControlComponent> control, ref GetItemActionsEvent args)
    {
        var (uid, comp) = control;

        if (comp.SealActionEntity == null || args.SlotFlags != comp.RequiredControlSlot)
            return;

        args.AddAction(comp.SealActionEntity.Value);
    }


    private void OnRelayedVerbRequest(Entity<SealableClothingControlComponent> control, ref InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>> args)
    {
        OnEquipmentVerb(control, ref args.Args);
    }
    /// <summary>
    /// Adds unsealing verbs to sealing control allowing other users to unseal/seal clothing via stripping
    /// </summary>
    private void OnEquipmentVerb(Entity<SealableClothingControlComponent> control, ref GetVerbsEvent<EquipmentVerb> args)
    {
        var (uid, comp) = control;
        var user = args.User;

        if (!args.CanComplexInteract)
            return;

        // Prevent Station AI from toggling modsuit seals
        if (HasComp<StationAiHeldComponent>(user))
            return;
        // Since sealing control in wearer's container system just won't show verb on args.CanAccess
        if (!_interactionSystem.InRangeUnobstructed(user, uid))
            return;

        if (comp.WearerEntity == null)
            return;

        var verbIcon = comp.IsCurrentlySealed ?
            new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/unlock.svg.192dpi.png")) :
            new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/lock.svg.192dpi.png"));

        var verb = new EquipmentVerb()
        {
            Icon = verbIcon,
            Priority = 5,
            Text = Loc.GetString(comp.VerbText),
        };

        if (args.User == comp.WearerEntity)
        {
            verb.Act = () => TryStartSealToggleProcess(control, user);
        }
        else
        {
            verb.Act = () => StartSealDoAfter(user, control, comp.WearerEntity.Value);
        }

        args.Verbs.Add(verb);
    }
    private void StartSealDoAfter(EntityUid user, Entity<SealableClothingControlComponent> control, EntityUid wearer)
    {
        _popupSystem.PopupClient("You start the suits' sealing process", wearer, user);
        var args = new DoAfterArgs(EntityManager, user, control.Comp.NonWearerSealingTime, new StartSealingProcessDoAfterEvent(), control, wearer, control)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2,
        };

        if (!_doAfterSystem.TryStartDoAfter(args))
        {
            return;
        }

        var popup = Loc.GetString("strippable-component-alert-owner-interact", ("user", Identity.Entity(user, EntityManager)), ("item", control));
        _popupSystem.PopupEntity(popup, wearer, wearer, PopupType.Large);

    }

    /// <summary>
    /// Ensure actionEntity on map init
    /// </summary>
    private void OnControlMapInit(Entity<SealableClothingControlComponent> control, ref MapInitEvent args)
    {
        var (uid, comp) = control;
        _actionContainerSystem.EnsureAction(uid, ref comp.SealActionEntity, comp.SealAction);
    }

    private void OnStartSealingDoAfter(Entity<SealableClothingControlComponent> control, ref StartSealingProcessDoAfterEvent args)
    {
        if (args.Cancelled)
            return;
        var user = args.User;
        // unless you have another way to do doafters inside doafters then yeah
        Timer.Spawn(0, () => TryStartSealToggleProcess(control, user));
    }

    /// <summary>
    /// Trying to start sealing on action. It'll notify wearer if process already started
    /// </summary>
    private void OnControlSealEvent(Entity<SealableClothingControlComponent> control, ref SealClothingEvent args)
    {
        var (uid, comp) = control;

        if (!_actionBlockerSystem.CanInteract(args.Performer, null))
            return;

        if (comp.IsInProcess)
        {
            _popupSystem.PopupClient(comp.IsCurrentlySealed
                    ? Loc.GetString(comp.SealedInProcessToggleFailPopup)
                    : Loc.GetString(comp.UnsealedInProcessToggleFailPopup),
                uid,
                args.Performer);

            _audioSystem.PlayPredicted(comp.FailSound, uid, args.Performer);

            return;
        }

        TryStartSealToggleProcess(control, args.Performer);
    }

    /// <summary>
    /// Toggle seal on one part and starts same process on next part
    /// </summary>
    private void OnSealClothingDoAfter(Entity<SealableClothingControlComponent> control, ref SealClothingDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        var part = args.Target;

        if (SealPart(part.Value, control, false))
            NextSealProcess(control);
    }

    public bool SealPart(Entity<SealableClothingComponent?> ent, Entity<SealableClothingControlComponent> control, bool silent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        var (uid, comp) = control;
        var (part, sealableComponet) = (ent.Owner, ent.Comp);

        sealableComponet.IsSealed = !comp.IsCurrentlySealed;

        Dirty(part, sealableComponet);

        if (!silent)
        {
            if (sealableComponet.IsSealed)
                _audioSystem.PlayPvs(sealableComponet.SealUpSound, uid);
            else
                _audioSystem.PlayPvs(sealableComponet.SealUpSound, uid);
        }

        _appearanceSystem.SetData(part, SealableClothingVisuals.Sealed, sealableComponet.IsSealed);

        var ev = new ClothingPartSealCompleteEvent(sealableComponet.IsSealed);
        RaiseLocalEvent(part, ref ev);

        return true;
    }

    /// <summary>
    /// Handles clothing toggling if it's sealed or in sealing process
    /// </summary>
    private void OnToggleClothingAttempt(Entity<SealableClothingControlComponent> control, ref ToggleClothingAttemptEvent args)
    {

        var (uid, comp) = control;
        var wearer = control.Comp.WearerEntity;

        if (wearer == null)
        {
            args.Cancel();
            return;
        }

        // Popup if currently sealing
        if (comp.IsInProcess)
        {
            _popupSystem.PopupClient(Loc.GetString(comp.UnsealedInProcessToggleFailPopup), uid, args.User);
            _audioSystem.PlayPvs(comp.FailSound, uid);
            args.Cancel();
            return;
        }

        // Seal after toggling for others
        var toggleStatus = _toggleableSystem.GetAttachedToggleStatus(wearer.Value, control, false);
        if (!comp.IsCurrentlySealed && toggleStatus == ToggleableClothingAttachedStatus.NoneToggled && wearer != args.User)
        {
            StartSealDoAfter(args.User, control, wearer.Value);
        }

        if (!comp.IsCurrentlySealed)
            return;

        // Popup for attempting to singular unequip with full seal
        if (!args.Multiple)
        {
            _popupSystem.PopupClient(Loc.GetString(comp.CurrentlySealedToggleFailPopup), uid, args.User);
            _audioSystem.PlayPvs(comp.FailSound, uid);
            args.Cancel();
            return;
        }

        // Otherwise its a multiple toggle so we start unseal process
        if (wearer == args.User)
        {
            comp.UnequipAfterUnseal = true;
            TryStartSealToggleProcess(control, args.User);
            args.Cancel();
            return;
        }
        comp.UnequipAfterUnseal = true;
        StartSealDoAfter(args.User, control, wearer.Value);
        args.Cancel();
    }
    #endregion

    /// <summary>
    ///     Tries to start sealing process
    /// </summary>
    /// <param name="control"></param>
    /// <returns></returns>
    public bool TryStartSealToggleProcess(Entity<SealableClothingControlComponent> control, EntityUid? user = null)
    {
        var (uid, comp) = control;

        // Prevent sealing/unsealing if modsuit don't have wearer or already started process
        if (comp.WearerEntity == null || comp.IsInProcess)
            return false;

        var wearer = comp.WearerEntity;

        var ev = new ClothingSealAttemptEvent(wearer.Value);
        RaiseLocalEvent(control, ev);

        if (ev.Cancelled)
            return false;

        // All parts required to be toggled to perform sealing
        // fix; now able to unseal even if all parts not toggled
        // edge cases where a sealed part may be unequipped and you get stuck with a broke suit
        // Sealbreaker along with OnAttachedUnequip in Toggleableclothing should take care if a sealed part unequips
        // but we still let the user manually unseal as a fallback to an impossible situation
        if (!comp.IsCurrentlySealed && _toggleableSystem.GetAttachedToggleStatus(wearer.Value, uid, false) != ToggleableClothingAttachedStatus.AllToggled)
        {
            if (user == wearer) // Popup spam prevent
            {
                _popupSystem.PopupClient(Loc.GetString(comp.ToggleFailedPopup), uid, user);
                _audioSystem.PlayPredicted(comp.FailSound, user.Value, user);
                return false;
            }
            if (_netManager.IsClient) // Popup spam prevent
                return false;
            _popupSystem.PopupEntity(Loc.GetString(comp.ToggleFailedPopup), uid);
            _audioSystem.PlayPvs(comp.FailSound, uid);
            return false;

        }

        // Trying to get all clothing to seal
        var sealeableList = _toggleableSystem.GetAttachedClothingsList(uid);
        if (sealeableList == null || sealeableList.Count == 0)
            return false;

        foreach (var sealeable in sealeableList)
        {
            if (!HasComp<SealableClothingComponent>(sealeable))
            {
                _popupSystem.PopupEntity(Loc.GetString(comp.ToggleFailedPopup), uid);
                _audioSystem.PlayPvs(comp.FailSound, uid);

                comp.ProcessQueue.Clear();
                Dirty(control);

                return false;
            }

            comp.ProcessQueue.Enqueue(EntityManager.GetNetEntity(sealeable));
        }

        comp.IsInProcess = true;
        Dirty(control);

        NextSealProcess(control);

        return true;
    }

    /// <summary>
    ///     Iteratively seals/unseals all parts of sealable clothing
    /// </summary>
    /// <param name="control"></param>
    private void NextSealProcess(Entity<SealableClothingControlComponent> control)
    {
        var (uid, comp) = control;
        while (true) // ugly but this used to be recursion so if we're doing this that way use iteration instead.
        {
            if (comp.WearerEntity is not { Valid: true } || !comp.IsInProcess) // dont just fucking assume the fucking entity will never be null what if a dev gibbs you at 3 in the fucking morning.
                return;

            // Finish sealing process
            if (comp.ProcessQueue.Count == 0)
            {
                EndSealProcess(control);
                return;
            }

            var processingPart = EntityManager.GetEntity(comp.ProcessQueue.Dequeue());
            Dirty(control);

            if (!TryComp<SealableClothingComponent>(processingPart, out var sealableComponent) || !comp.IsInProcess)
            {
                _popupSystem.PopupClient(Loc.GetString(comp.ToggleFailedPopup), uid, comp.WearerEntity);
                _audioSystem.PlayPredicted(comp.FailSound, uid, comp.WearerEntity);

                continue;
            }

            // If part is sealed when control trying to seal - it should just skip this part
            if (sealableComponent.IsSealed != comp.IsCurrentlySealed)
                continue;

            var doAfterArgs = new DoAfterArgs(EntityManager, uid, sealableComponent.SealingTime, new SealClothingDoAfterEvent(), uid, target: processingPart, showTo: comp.WearerEntity) { NeedHand = false, RequireCanInteract = false, };

            // Checking for client here to skip first process popup spam that happens. Predicted popups don't work here because doafter starts on sealable control, not on player.
            if (!_doAfterSystem.TryStartDoAfter(doAfterArgs) || _netManager.IsClient)
                return;

            // This is mostly for faster seal unseal times so that the popups dont overlay on eachother
            var xform = Transform(comp.WearerEntity.Value);
            var baseCoords = xform.Coordinates;
            var offsetY = 0.25f * comp.ProcessQueue.Count;
            var popupCoords = baseCoords.Offset(new Vector2(0f, offsetY));

            var popupText = Loc.GetString(
                comp.IsCurrentlySealed ? sealableComponent.SealDownPopup : sealableComponent.SealUpPopup,
                ("partName", Identity.Name(processingPart, EntityManager))
            );
            var type = comp.IsCurrentlySealed ?  PopupType.SmallCaution :  PopupType.Small;
            _popupSystem.PopupCoordinates(popupText, popupCoords, comp.WearerEntity.Value, type);

            break;
        }
    }

    /// <summary>
    ///     Finishes sealing process on control
    /// </summary>
    public void EndSealProcess(Entity<SealableClothingControlComponent> control, bool silent = false)
    {
        var (uid, comp) = control;
        // if this system was more foolproof we could swap it around as we did
        // but no so much shit can fuck up
        // so just actually sanity check the parts when the process is done.
        var attachedParts = _toggleableSystem.GetAttachedClothingsList(uid);
        var allpartsSealed = true;
        if (attachedParts == null)
            return;

        foreach (var part in attachedParts)
        {
            if (TryComp<SealableClothingComponent>(part, out var pSeal) && pSeal.IsSealed)
                    continue;
            allpartsSealed = false;
            break;
        }

        comp.IsCurrentlySealed = allpartsSealed;
        // if you gib or remove while sound plays it throws exception so yeah we DO CHECK IF NULL
        if (comp.WearerEntity is not { Valid: true })
            return;

        if (!silent)
        {
            _audioSystem.PlayEntity(comp.IsCurrentlySealed ? comp.SealCompleteSound : comp.UnsealCompleteSound,
                comp.WearerEntity.Value,
                uid);
        }

        var ev = new ClothingControlSealCompleteEvent(comp.IsCurrentlySealed);
        RaiseLocalEvent(control, ref ev);
        _appearanceSystem.SetData(uid, SealableClothingVisuals.Sealed, comp.IsCurrentlySealed);
        comp.IsInProcess = false;
        Dirty(control);
    }

    private void OnBackClothingUnequipped(Entity<SealableClothingControlComponent> control, ref ToggledBackClothingFullUnequipAndInsertedEvent args)
    {
        var comp = control.Comp;

        // Check if it's in the middle of sealing/unsealing
        if (!comp.IsInProcess || comp.UnequipAfterUnseal) // yes im looking at unequipafterunseal here to make sure we dont seal after untoggling with that bad way of doing it earlier.
            return;

        comp.ProcessQueue.Clear();
        comp.IsInProcess = false;
        // Force all sealed parts to sealed state immediately
        var attachedParts = _toggleableSystem.GetAttachedClothingsList(control.Owner);
        if (attachedParts == null)
            return;

        foreach (var part in attachedParts)
        {
            if (!TryComp<SealableClothingComponent>(part, out var partSeal))
                continue;
            partSeal.IsSealed = true;
            Dirty(part, partSeal);
            _appearanceSystem.SetData(part, SealableClothingVisuals.Sealed, true);
        }
        SealBreaker(control);
    }

    private void SealBreaker(EntityUid controlUid)
    {
        if (!TryComp<SealableClothingControlComponent>(controlUid, out var comp))
            return;

        if (comp is { IsInProcess: false, IsCurrentlySealed: false })
            return;

        comp.ProcessQueue.Clear();
        comp.IsCurrentlySealed = false;
        comp.IsInProcess = false;
        Dirty(controlUid, comp);

        _appearanceSystem.SetData(controlUid, SealableClothingVisuals.Sealed, false);

        var attached = _toggleableSystem.GetAttachedClothingsList(controlUid);
        if (attached == null || attached.Count == 0)
            return;

        foreach (var part in attached)
        {
            if (!TryComp(part, out SealableClothingComponent? partSeal) || !partSeal.IsSealed)
                continue;

            partSeal.IsSealed = false;
            Dirty(part, partSeal);
            _appearanceSystem.SetData(part, SealableClothingVisuals.Sealed, false);
        }
        if (comp.WearerEntity == null)
            return;
        _popupSystem.PopupEntity(Loc.GetString(comp.SealBrokenPopup), controlUid, comp.WearerEntity.Value, PopupType.LargeCaution);
        _audioSystem.PlayPvs(comp.GenericSuitWarning, controlUid);
    }
    private void OnAttachedUnequipAttemptSealCheck(Entity<SealableClothingComponent> attached, ref OnAttachedUnequipAttemptEvent args)
    {

        var toggleableEnt = args.Toggleable;
        // Cancel if the control unit is sealed.
        if (TryComp<SealableClothingControlComponent>(toggleableEnt, out var controlSeal) && controlSeal.IsCurrentlySealed || controlSeal!.IsInProcess)
        {
            _popupSystem.PopupClient(Loc.GetString("sealable-clothing-sealed-toggle-fail"), toggleableEnt, args.UnEquipTarget);
            args.Cancel();
            return;
        }

        if (!TryComp<ToggleableClothingComponent>(attached, out var toggleableComp))
            return;

        // Cancel if any parts are sealed.
        foreach (var partUid in toggleableComp.ClothingUids.Keys)
        {
            if (!TryComp<SealableClothingComponent>(partUid, out var partSeal) || !partSeal.IsSealed)
                continue;
            _popupSystem.PopupClient(Loc.GetString("sealable-clothing-sealed-toggle-fail"), toggleableEnt, args.UnEquipTarget);
            args.Cancel();
            return;
        }
    }

    private void OnToggleableUnequipAttemptSealCheck(Entity<SealableClothingControlComponent> toggleable, ref BeingUnequippedAttemptEvent args)
    {
        var toggleableEnt = toggleable.Owner;
        // Cancel if the control unit is sealed.
        if (TryComp<SealableClothingControlComponent>(toggleableEnt, out var controlSeal) && controlSeal.IsCurrentlySealed || controlSeal!.IsInProcess)
        {
            _popupSystem.PopupClient(Loc.GetString("sealable-clothing-sealed-toggle-fail"), toggleableEnt, args.Unequipee);
            args.Cancel();
            return;
        }

        if (!TryComp<ToggleableClothingComponent>(toggleable, out var toggleableComp))
            return;

        // Cancel if any parts are sealed.
        foreach (var partUid in toggleableComp.ClothingUids.Keys)
        {
            if (!TryComp<SealableClothingComponent>(partUid, out var partSeal) || !partSeal.IsSealed)
                continue;
            _popupSystem.PopupClient(Loc.GetString("sealable-clothing-sealed-toggle-fail"), toggleableEnt, args.Unequipee);
            args.Cancel();
            return;
        }
    }
    private void OnToggleSanityChecker(Entity<SealableClothingControlComponent> sealable, ref OnToggleableUnequipAttemptEvent args)
    {
        // AttachedUid = Toggleable Part | args.Toggleable
        // Owner       = Toggled Part    | args.Attached
        if (!TryComp<SealableClothingComponent>(args.Attached, out var sealableComp))
            return;

        if (!sealableComp.IsSealed)
            return;

        var inSlot = _inventorySystem.TryGetContainingSlot(args.Attached, out var slot);

        if (inSlot && slot != null)
            return;
        SealBreaker(args.Toggleable);
    }
}



[Serializable, NetSerializable]
public sealed partial class SealClothingDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class StartSealingProcessDoAfterEvent : SimpleDoAfterEvent
{
}

public sealed partial class SealClothingEvent : InstantActionEvent
{
}

/// <summary>
///     Raises on control when clothing finishes it's sealing or unsealing process
/// </summary>
[ByRefEvent]
public readonly record struct ClothingControlSealCompleteEvent(bool IsSealed)
{
    public readonly bool IsSealed = IsSealed;
}

/// <summary>
///     Raises on part when clothing finishes it's sealing or unsealing process
/// </summary>
[ByRefEvent]
public readonly record struct ClothingPartSealCompleteEvent(bool IsSealed)
{
    public readonly bool IsSealed = IsSealed;
}

public sealed class ClothingSealAttemptEvent(EntityUid user) : CancellableEntityEventArgs
{
    public EntityUid User = user;
}
