// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Goobstation.Shared.Xenobiology.Components.Equipment;
using Content.Shared.ActionBlocker;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

/// <summary>
/// This handles the XenoVacuum and it's interactions.
/// </summary>
public sealed partial class XenoVacuumSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly EmagSystem _emag = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private ISawmill _sawmill = default!;

    private EntityQuery<HumanoidAppearanceComponent> _humanoidQuery;
    private EntityQuery<MobStateComponent> _mobQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoVacuumTankComponent, ComponentInit>(OnTankInit);
        SubscribeLocalEvent<XenoVacuumTankComponent, ExaminedEvent>(OnTankExamined);

        SubscribeLocalEvent<XenoVacuumComponent, GotEmaggedEvent>(OnVacEmagged);

        SubscribeLocalEvent<XenoVacuumComponent, GotEquippedHandEvent>(OnEquippedHand);
        SubscribeLocalEvent<XenoVacuumComponent, GotUnequippedHandEvent>(OnUnequippedHand);

        SubscribeLocalEvent<XenoVacuumComponent, AfterInteractEvent>(OnXenoVacuum);

        _sawmill = Logger.GetSawmill("Xenobiology");

        _humanoidQuery = GetEntityQuery<HumanoidAppearanceComponent>();
        _mobQuery = GetEntityQuery<MobStateComponent>();
    }

    private void OnTankInit(Entity<XenoVacuumTankComponent> tank, ref ComponentInit args) =>
        tank.Comp.StorageTank = _containerSystem.EnsureContainer<Container>(tank, "StorageTank");

    private void OnTankExamined(Entity<XenoVacuumTankComponent> tank, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        foreach (var ent in tank.Comp.StorageTank.ContainedEntities)
        {
            var text = Robust.Shared.Localization.Loc.GetString("xeno-vacuum-examined", ("ent", ent));
            args.PushMarkup(text);
        }
    }

    private void OnEquippedHand(Entity<XenoVacuumComponent> vacuum, ref GotEquippedHandEvent args)
    {
        if (!_inventorySystem.TryGetSlotEntity(args.User, "suitstorage", out var tank)
            || !TryComp<XenoVacuumTankComponent>(tank, out var tankComp)
            || _net.IsClient)
            return;

        tankComp.LinkedNozzle = vacuum;

        Dirty(vacuum);
        Dirty(tank.Value, tankComp);
    }

    private void OnUnequippedHand(Entity<XenoVacuumComponent> vacuum, ref GotUnequippedHandEvent args)
    {
        if (!_inventorySystem.TryGetSlotEntity(args.User, "suitstorage", out var tank)
            || !TryComp<XenoVacuumTankComponent>(tank, out var tankComp)
            || _net.IsClient)
            return;

        tankComp.LinkedNozzle = null;

        Dirty(vacuum);
        Dirty(tank.Value, tankComp);
    }

    private void OnVacEmagged(Entity<XenoVacuumComponent> vacuum, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction)
            || _emag.CheckFlag(vacuum, EmagType.Interaction)
            || HasComp<EmaggedComponent>(vacuum)
            || _net.IsClient)
            return;

        args.Handled = true;
    }

    private void OnXenoVacuum(Entity<XenoVacuumComponent> ent, ref AfterInteractEvent args)
    {
        if (_net.IsClient)
            return;

        if (args is { Target: { } target, CanReach: true }
            && HasComp<MobStateComponent>(target))
        {
            TryDoSuction(args.User, target, ent);
            return;
        }

        if (!_inventorySystem.TryGetSlotEntity(args.User, "suitstorage", out var backSlotEntity)
            || !TryComp<XenoVacuumTankComponent>(backSlotEntity, out var tankComp)
            || tankComp.StorageTank.ContainedEntities.Count <= 0)
            return;

        foreach (var removedEnt in _containerSystem.EmptyContainer(tankComp.StorageTank))
        {
            var popup = Loc.GetString("xeno-vacuum-clear-popup", ("ent", removedEnt));
            _popup.PopupEntity(popup, ent, args.User);

            if (args.Target is { } thrown)
                _throw.TryThrow(removedEnt, thrown.ToCoordinates());
            else
                _throw.TryThrow(removedEnt, args.ClickLocation);

            _stun.TryParalyze(removedEnt, TimeSpan.FromSeconds(2), true);
        }

        _audio.PlayEntity(ent.Comp.ClearSound, ent, args.User, AudioParams.Default.WithVolume(-2f));
    }

    #region Helpers

    private bool TryDoSuction(EntityUid user, EntityUid target, Entity<XenoVacuumComponent> vacuum)
    {
        if (!_inventorySystem.TryGetSlotEntity(user, "suitstorage", out var tank)
            || !TryComp<XenoVacuumTankComponent>(tank, out var tankComp)
            || _net.IsClient)
        {
            var noTankPopup = Loc.GetString("xeno-vacuum-suction-fail-no-tank-popup");
            _popup.PopupEntity(noTankPopup, vacuum, user);

            return false;
        }

        if (!_mobQuery.HasComp(target)
            || _humanoidQuery.HasComp(target) && !HasComp<EmaggedComponent>(vacuum))
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

        if (!_containerSystem.Insert(target, tankComp.StorageTank))
        {
            _sawmill.Debug($"{ToPrettyString(user)} failed to insert {ToPrettyString(target)} into {ToPrettyString(tank)}");
            return false;
        }

        _audio.PlayEntity(vacuum.Comp.Sound, user, user);

        var successPopup = Loc.GetString("xeno-vacuum-suction-succeed-popup", ("ent", target));
        _popup.PopupEntity(successPopup, vacuum, user);

        return true;
    }


    #endregion

}
