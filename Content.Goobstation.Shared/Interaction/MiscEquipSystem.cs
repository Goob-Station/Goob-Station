// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 yavuz <58685802+yahay505@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.ActionBlocker;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Implants.Components;
using Content.Shared.Input;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Interaction;

public sealed class MiscEquipSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;


    public override void Initialize()
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.SmartEquipSlimeStorage,
                InputCmdHandler.FromDelegate(HandleEquipToSlimeStorage,
                    handle: false,
                    outsidePrediction: false)) // Goobstation - Smart equip to slime storage
            .Bind(ContentKeyFunctions.SmartEquipStorageImplant,
                InputCmdHandler.FromDelegate(HandleEquipToStorageImplant,
                    handle: false,
                    outsidePrediction: false)) // Goobstation - Smart equip to storage implant
            .Register<MiscEquipSystem>();
    }

    private void HandleEquipToStorageImplant(ICommonSession? session)
    {
        if (session is not { } playerSession)
            return;

        if (playerSession.AttachedEntity is not { Valid: true } uid || !Exists(uid))
            return;

        if (!TryComp<HandsComponent>(uid, out var hands) || hands.ActiveHand == null)
            return;

        var handItem = hands.ActiveHand.HeldEntity;

        if (!_actionBlocker.CanInteract(uid, handItem))
            return;
        StorageComponent? storage=null;
        EntityUid implantHolderUid = default;
        if (TryComp<ImplantedComponent>(uid, out var implantedComp))
        {

            var implantContainer = _container.EnsureContainer<Container>(uid,"implant");
            if (implantContainer==null)
                goto afterContainerSearch; // early out bcs implanted comp can have no implant container? wtf?

            var implants = implantContainer.ContainedEntities;

            foreach (var implant in implants)
            {
                if (TryComp(implant, out StorageComponent? comp))
                {
                    storage = comp;
                    implantHolderUid = implant;
                    break;
                }
            }
        }
        afterContainerSearch:

        if (storage==null)
        {
            _popup.PopupClient(Loc.GetString("smart-equip-missing-equipment-slot", ("slotName", "storage implant")),
                uid,
                uid);
            return;
        }

        if (handItem != null && !_hands.CanDropHeld(uid, hands.ActiveHand))
        {
            _popup.PopupClient(Loc.GetString("smart-equip-cant-drop"), uid, uid);
            return;
        }

        switch (handItem)
        {
            case null when storage.Container.ContainedEntities.Count == 0:
                _popup.PopupClient(Loc.GetString("smart-equip-empty-equipment-slot", ("slotName", "storage implant")), uid, uid);
                return;
            case null:
                var removing = storage.Container.ContainedEntities[^1];
                _container.RemoveEntity(implantHolderUid, removing);
                _hands.TryPickup(uid, removing, handsComp: hands);
                return;
        }

        if (!_storage.CanInsert(implantHolderUid, handItem.Value, out var reason))
        {
            if (reason != null)
                _popup.PopupClient(Loc.GetString(reason), uid, uid);

            return;
        }

        _hands.TryDrop(uid, hands.ActiveHand, handsComp: hands);
        _storage.Insert(implantHolderUid, handItem.Value, out var stacked, out _);

        // if the hand item stacked with the things in inventory, but there's no more space left for the rest
        // of the stack, place the stack back in hand rather than dropping it on the floor
        if (stacked != null && !_storage.CanInsert(uid, handItem.Value, out _))
        {
            if (TryComp<StackComponent>(handItem.Value, out var handStack) && handStack.Count > 0)
                _hands.TryPickup(uid, handItem.Value, handsComp: hands);
        }

        return;
    }

    private void HandleEquipToSlimeStorage(ICommonSession? session)
    {
        if (session is not { } playerSession)
            return;

        if (playerSession.AttachedEntity is not { Valid: true } uid || !Exists(uid))
            return;

        if (!TryComp<HandsComponent>(uid, out var hands) || hands.ActiveHand == null)
            return;

        var handItem = hands.ActiveHand.HeldEntity;

        if (!_actionBlocker.CanInteract(uid, handItem))
            return;
        if (!TryComp<StorageComponent>(uid, out var slimestorage))
        {
            _popup.PopupClient(Loc.GetString("smart-equip-missing-equipment-slot", ("slotName", "slime storage")),
                uid,
                uid);
            return;
        }

        if (handItem != null && !_hands.CanDropHeld(uid, hands.ActiveHand))
        {
            _popup.PopupClient(Loc.GetString("smart-equip-cant-drop"), uid, uid);
            return;
        }

        var  storage = slimestorage!;
        var emptyEquipmentSlotString = Loc.GetString("smart-equip-empty-equipment-slot", ("slotName", "slime storage"));


        switch (handItem)
        {
            case null when storage.Container.ContainedEntities.Count == 0:
                _popup.PopupClient(emptyEquipmentSlotString, uid, uid);
                return;
            case null:
                var removing = storage.Container.ContainedEntities[^1];
                _container.RemoveEntity(uid, removing);
                _hands.TryPickup(uid, removing, handsComp: hands);
                return;
        }

        if (!_storage.CanInsert(uid, handItem.Value, out var reason))
        {
            if (reason != null)
                _popup.PopupClient(Loc.GetString(reason), uid, uid);

            return;
        }

        _hands.TryDrop(uid, hands.ActiveHand, handsComp: hands);
        _storage.Insert(uid, handItem.Value, out var stacked, out _);

        // if the hand item stacked with the things in inventory, but there's no more space left for the rest
        // of the stack, place the stack back in hand rather than dropping it on the floor
        if (stacked != null && !_storage.CanInsert(uid, handItem.Value, out _))
        {
            if (TryComp<StackComponent>(handItem.Value, out var handStack) && handStack.Count > 0)
                _hands.TryPickup(uid, handItem.Value, handsComp: hands);
        }
        return;
    }
    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<MiscEquipSystem>();
    }
}
