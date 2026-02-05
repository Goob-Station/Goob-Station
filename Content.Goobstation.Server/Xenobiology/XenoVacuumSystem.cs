// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.Components.Equipment;
using Content.Server.NPC.HTN;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Coordinates;
using Content.Shared.Destructible;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Timing;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles the XenoVacuum and it's interactions.
/// </summary>
public sealed partial class XenoVacuumSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly EmagSystem _emag = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly EntityStorageSystem _entStorage = default!;

    private const string ReleaseDelayId = "release";
    private const string SuctionDelayId = "suction";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenoVacuumTankComponent, MapInitEvent>(OnTankInit);
        SubscribeLocalEvent<XenoVacuumTankComponent, ExaminedEvent>(OnTankExamined);
        SubscribeLocalEvent<XenoVacuumTankComponent, DestructionEventArgs>(OnDestruction);

        SubscribeLocalEvent<XenoVacuumComponent, GotEmaggedEvent>(OnGotEmagged);
        SubscribeLocalEvent<XenoVacuumComponent, GotEquippedHandEvent>(OnEquippedHand);
        SubscribeLocalEvent<XenoVacuumComponent, GotUnequippedHandEvent>(OnUnequippedHand);
        SubscribeLocalEvent<XenoVacuumComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnTankInit(Entity<XenoVacuumTankComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.StorageTank = _containerSystem.EnsureContainer<Container>(ent, ent.Comp.TankContainerName);
    }

    private void OnTankExamined(Entity<XenoVacuumTankComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || !TryComp<EntityStorageComponent>(ent, out var entStorage))
            return;

        var text = Loc.GetString("xeno-vacuum-examined", ("n", entStorage.Contents.ContainedEntities.Count));
        args.PushMarkup(text);
    }

    private void OnDestruction(Entity<XenoVacuumTankComponent> ent, ref DestructionEventArgs args)
    {
        // apparently ContainerManager doesn't automatically release them so
        _containerSystem.EmptyContainer(ent.Comp.StorageTank);
    }

    private void OnEquippedHand(Entity<XenoVacuumComponent> ent, ref GotEquippedHandEvent args)
    {
        if (!TryGetTank(args.User, out var tank) && !tank.HasValue)
            return;

        var tankComp = tank!.Value.Comp;

        tankComp.LinkedNozzle = ent;

        Dirty(ent);
        Dirty(tank.Value, tankComp);
    }

    private void OnUnequippedHand(Entity<XenoVacuumComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (!TryGetTank(args.User, out var tank) && !tank.HasValue)
            return;

        var tankComp = tank!.Value.Comp;

        tankComp.LinkedNozzle = null;

        Dirty(ent);
        Dirty(tank.Value, tankComp);
    }

    private void OnGotEmagged(Entity<XenoVacuumComponent> ent, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction)
        || _emag.CheckFlag(ent, EmagType.Interaction)
        || HasComp<EmaggedComponent>(ent))
            return;

        args.Handled = true;
    }

    private void OnAfterInteract(Entity<XenoVacuumComponent> ent, ref AfterInteractEvent args)
    {
        var ud = Comp<UseDelayComponent>(ent);
        if (CheckDelays(ent)) return;

        if (args is { Target: { } target, CanReach: true } && HasComp<MobStateComponent>(target))
        {
            TryDoSuction(args.User, target, ent);
            if (ud != null) _useDelay.TryResetDelay((ent, ud), false, SuctionDelayId);
            return;
        }

        // release all entities contained
        if (!TryGetTank(args.User, out var tank) || !tank.HasValue
        || tank!.Value.Comp.StorageTank.ContainedEntities.Count <= 0)
            return;

        var tankComp = tank!.Value.Comp;

        foreach (var removedEnt in _containerSystem.EmptyContainer(tankComp.StorageTank))
        {
            var popup = Loc.GetString("xeno-vacuum-clear-popup", ("ent", removedEnt));
            _popup.PopupEntity(popup, ent, args.User);

            if (args.Target is { } thrown) _throw.TryThrow(removedEnt, thrown.ToCoordinates());
            else _throw.TryThrow(removedEnt, args.ClickLocation);

            _stun.TryKnockdown(removedEnt, TimeSpan.FromSeconds(1), true);
            _htn.SetHTNEnabled(removedEnt, true, 1f);
        }

        if (ud != null) _useDelay.TryResetDelay((ent, ud), false, ReleaseDelayId);

        _audio.PlayEntity(ent.Comp.ClearSound, ent, args.User, AudioParams.Default.WithVolume(-2f));
    }

    #region Helpers

    private bool CheckDelays(Entity<XenoVacuumComponent> tank)
        => _useDelay.IsDelayed(tank.Owner, SuctionDelayId)
        || _useDelay.IsDelayed(tank.Owner, ReleaseDelayId);

    private bool TryGetTank(EntityUid user, out Entity<XenoVacuumTankComponent>? tank)
    {
        tank = null;

        foreach (var item in _hands.EnumerateHeld(user))
        {
            if (!TryComp<XenoVacuumTankComponent>(item, out var xenoVacTank))
                continue;
            tank = (item, xenoVacTank);
            return true;
        }

        if (!_inventorySystem.TryGetContainerSlotEnumerator(user, out var slotEnum, SlotFlags.WITHOUT_POCKET))
            return false;

        while (slotEnum.MoveNext(out var item))
        {
            if (!item.ContainedEntity.HasValue)
                continue;

            if (!TryComp<XenoVacuumTankComponent>(item.ContainedEntity.Value, out var xenoVacTank))
                continue;
            tank = (item.ContainedEntity.Value, xenoVacTank);
            return true;
        }

        return false;
    }

    private bool TryDoSuction(EntityUid user, EntityUid target, Entity<XenoVacuumComponent> vacuum)
    {
        if (!TryGetTank(user, out var tank) || !tank.HasValue)
        {
            var noTankPopup = Loc.GetString("xeno-vacuum-suction-fail-no-tank-popup");
            _popup.PopupEntity(noTankPopup, vacuum, user);

            return false;
        }

        var tankComp = tank.Value.Comp;

        if (!HasComp<EmaggedComponent>(vacuum) && !_whitelist.IsWhitelistPass(vacuum.Comp.EntityWhitelist, target))
        {
            var invalidEntityPopup = Loc.GetString("xeno-vacuum-suction-fail-invalid-entity-popup", ("ent", target));
            _popup.PopupEntity(invalidEntityPopup, vacuum, user);

            return false;
        }

        if (tankComp.StorageTank.ContainedEntities.Count > tankComp.MaxEntities)
        {
            var tankFullPopup = Loc.GetString("xeno-vacuum-suction-fail-tank-full-popup");
            _popup.PopupEntity(tankFullPopup, vacuum, user);

            return false;
        }

        _htn.SetHTNEnabled(target, false);

        if (!_containerSystem.Insert(target, tankComp.StorageTank))
        {
            Log.Debug($"{ToPrettyString(user)} failed to insert {ToPrettyString(target)} into {ToPrettyString(tank)}");
            return false;
        }

        _audio.PlayEntity(vacuum.Comp.Sound, user, user);
        var successPopup = Loc.GetString("xeno-vacuum-suction-succeed-popup", ("ent", target));
        _popup.PopupEntity(successPopup, vacuum, user);

        return true;
    }

    #endregion
}
