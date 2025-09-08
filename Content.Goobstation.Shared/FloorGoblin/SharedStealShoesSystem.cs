// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.FloorGoblin;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Goobstation.Shared.FloorGoblin;

public abstract partial class SharedStealShoesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ITileDefinitionManager _tileManager = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

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

        if (_net.IsClient)
        {
            args.Handled = true;
            return;
        }

        var target = args.Target;

        if (!_interaction.InRangeUnobstructed(uid, target))
            return;

        if (!CanStealHere(uid))
        {
            _popup.PopupEntity(Loc.GetString("steal-shoes-covered"), uid, uid);
            args.Handled = true;
            return;
        }

        if (!_inventory.TryGetSlotEntity(target, "shoes", out var shoesUid) || shoesUid == null)
        {
            _popup.PopupEntity(Loc.GetString("steal-shoes-no-shoes"), uid, uid);
            args.Handled = true;
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

        if (ev.Cancelled || ev.Args.Target is not { } target)
            return;

        if (!CanStealHere(uid))
            return;

        if (!_inventory.TryGetSlotEntity(target, "shoes", out var shoesUid) || shoesUid is not { } shoes)
            return;

        if (!_inventory.TryUnequip(target, "shoes"))
            return;

        if (!_containers.TryGetContainer(uid, component.ContainerId, out var container))
            container = _containers.EnsureContainer<Container>(uid, component.ContainerId);

        _containers.Insert(shoes, container);

        if (component.ChompSound is { } chomp)
            _audio.PlayPvs(chomp, uid);

        _popup.PopupEntity(Loc.GetString("shoes-stolen-target-event"), target, target);
        _popup.PopupEntity(Loc.GetString("steal-shoes-event", ("target", MetaData(target).EntityName), ("shoes", MetaData(shoes).EntityName)), uid, uid);
    }

    private void OnMobStateChanged(EntityUid uid, StealShoesComponent component, MobStateChangedEvent args)
    {
        if (!_net.IsServer)
            return;

        if (args.NewMobState != MobState.Dead)
            return;

        if (_containers.TryGetContainer(uid, component.ContainerId, out var container))
        {
            var dropCoords = Transform(uid).Coordinates;
            var toDrop = new List<EntityUid>(container.ContainedEntities);
            foreach (var ent in toDrop)
            {
                _containers.Remove(ent, container);
                _transform.SetCoordinates(ent, dropCoords);
                var angle = _random.NextFloat(0f, MathF.Tau);
                var speed = _random.NextFloat(2.5f, 4.5f);
                var vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                if (TryComp<PhysicsComponent>(ent, out var phys))
                    _physics.SetLinearVelocity(ent, vel);
            }
        }

        _body.GibBody(uid);
    }

    public bool IsOnSubfloor(EntityUid uid)
    {
        var xform = Transform(uid);
        if (_transform.GetGrid(xform.Coordinates) is not { } gridUid)
            return false;
        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return false;
        var snapPos = _map.TileIndicesFor((gridUid, grid), xform.Coordinates);
        var tileRef = _map.GetTileRef(gridUid, grid, snapPos);
        if (tileRef.Tile.IsEmpty)
            return false;
        var tileDef = (ContentTileDefinition) _tileManager[tileRef.Tile.TypeId];
        return tileDef.IsSubFloor;
    }

    private bool CanStealHere(EntityUid uid)
    {
        if (!TryComp<CrawlUnderFloorComponent>(uid, out var crawl) || !crawl.Enabled)
            return true;
        return IsOnSubfloor(uid);
    }


}
