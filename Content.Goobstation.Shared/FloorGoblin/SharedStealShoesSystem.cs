// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Numerics;
using Content.Goobstation.Common.FloorGoblin;
using Content.Goobstation.Shared.FloorGoblin;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.FloorGoblin;

public abstract partial class SharedStealShoesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ITileDefinitionManager _tileManager = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StealShoesComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<StealShoesComponent, StealShoesEvent>(OnStealShoes);
        SubscribeLocalEvent<StealShoesComponent, StealShoesDoAfterEvent>(OnStealShoesDoAfter);
        SubscribeLocalEvent<StealShoesComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnInit(EntityUid uid, StealShoesComponent component, ComponentInit args)
    {
        if (!_net.IsServer)
            return;

        if (component.StealAction == null)
            _actions.AddAction(uid, ref component.StealAction, component.ActionProto);

        _containers.EnsureContainer<Container>(uid, component.ContainerId);
    }

    private void OnStealShoes(EntityUid uid, StealShoesComponent component, StealShoesEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (!_interaction.InRangeUnobstructed(uid, target))
            return;

        if (!CanStealHere(uid))
        {
            _popup.PopupPredicted(Loc.GetString("steal-shoes-covered"), uid, uid);
            return;
        }

        if (!_inventory.TryGetSlotEntity(target, "shoes", out var shoesUid) || shoesUid == null)
        {
            _popup.PopupPredicted(Loc.GetString("steal-shoes-no-shoes"), uid, uid);
            return;
        }

        var dargs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(2), new StealShoesDoAfterEvent(), uid, target)
        {
            DistanceThreshold = 1.5f,
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnWeightlessMove = true,
            NeedHand = false,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
            DuplicateCondition = DuplicateConditions.SameEvent
        };

        if (_doAfter.TryStartDoAfter(dargs))
            args.Handled = true;
    }

    private void OnStealShoesDoAfter(EntityUid uid, StealShoesComponent component, ref StealShoesDoAfterEvent ev)
    {
        if (!_net.IsServer)
            return;
        if (ev.Cancelled || ev.Args.Target == null)
            return;
        if (!CanStealHere(uid))
            return;

        var target = ev.Args.Target.Value;

        if (!_inventory.TryGetSlotEntity(target, "shoes", out var shoesUid) || shoesUid == null)
            return;

        if (!_inventory.TryUnequip(target, "shoes"))
            return;

        if (!_containers.TryGetContainer(uid, component.ContainerId, out var container))
            container = _containers.EnsureContainer<Container>(uid, component.ContainerId);

        _containers.Insert(shoesUid.Value, container);

        if (component.ChompSound != null)
            _audio.PlayPvs(component.ChompSound, uid);

        _popup.PopupEntity(Loc.GetString("shoes-stolen-target-event"), target);
        _popup.PopupEntity(Loc.GetString("steal-shoes-event", ("shoes", MetaData(target).EntityName)), uid);
    }

    private void OnMobStateChanged(EntityUid uid, StealShoesComponent component, MobStateChangedEvent args)
    {
        if (!_net.IsServer)
            return;

        if (args.NewMobState != MobState.Dead)
            return;

        OnDeathServer(uid, component);
    }

    protected virtual void OnDeathServer(EntityUid uid, StealShoesComponent component) { }

    private bool IsOnSubfloor(EntityUid uid)
    {
        var x = Transform(uid);
        var gridUid = _xform.GetGrid(x.Coordinates);
        if (gridUid == null)
            return false;
        if (!TryComp<MapGridComponent>(gridUid.Value, out var grid))
            return false;

        var snap = _map.TileIndicesFor((gridUid.Value, grid), x.Coordinates);
        var tileRef = _map.GetTileRef(gridUid.Value, grid, snap);
        if (tileRef.Tile.IsEmpty)
            return false;

        var def = (ContentTileDefinition) _tileManager[tileRef.Tile.TypeId];
        return def.IsSubFloor;
    }

    private bool CanStealHere(EntityUid uid)
    {
        if (!TryComp<CrawlUnderFloorComponent>(uid, out var crawl) || !crawl.Enabled)
            return true;
        return IsOnSubfloor(uid);
    }
}
