// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.FloorGoblin;
using Content.Goobstation.Shared.FloorGoblin;
using Content.Server.Body.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Mind;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Server.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Goobstation.Server.FloorGoblin;

public sealed partial class StealShoesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ITileDefinitionManager _tileManager = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

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
        if (component.StealAction == null)
            _actions.AddAction(uid, ref component.StealAction, component.ActionProto);
        _containers.EnsureContainer<Container>(uid, component.ContainerId);
    }

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

    private int GetActivePlayers()
    {
        var count = 0;
        foreach (var s in _players.Sessions)
            if (s.Status == SessionStatus.InGame)
                count++;
        return count;
    }

    private void EnsureObjectiveRequired(EntityUid uid)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;
        if (!_mind.TryGetObjectiveComp<CollectShoesConditionComponent>(mindId, out var cond, mind))
            return;
        if (cond.Required > 0)
            return;

        var target = (int) Math.Ceiling(cond.Base + cond.PerPlayer * GetActivePlayers());
        if (target < cond.Min) target = cond.Min;
        if (target > cond.Max) target = cond.Max;
        cond.Required = target;
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
            _popup.PopupEntity(Loc.GetString("steal-shoes-covered"), uid);
            return;
        }

        EntityUid? shoesUid;
        if (!_inventory.TryGetSlotEntity(target, "shoes", out shoesUid) || shoesUid == null)
        {
            _popup.PopupEntity(Loc.GetString("steal-shoes-no-shoes"), uid);
            return;
        }

        EnsureObjectiveRequired(uid);

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
        if (ev.Cancelled)
            return;

        if (ev.Args.Target == null)
            return;

        if (!CanStealHere(uid))
            return;

        var target = ev.Args.Target.Value;

        EntityUid? shoesUid;
        if (!_inventory.TryGetSlotEntity(target, "shoes", out shoesUid) || shoesUid == null)
            return;

        if (!_inventory.TryUnequip(target, "shoes"))
            return;

        if (_containers.TryGetContainingContainer(shoesUid.Value, out var existing))
            _hands.TryDrop(target, shoesUid.Value, null, false);

        if (_containers.TryGetContainingContainer(shoesUid.Value, out existing))
            _containers.Remove(shoesUid.Value, existing);

        if (!_containers.TryGetContainer(uid, component.ContainerId, out var container))
            container = _containers.EnsureContainer<Container>(uid, component.ContainerId);

        _containers.Insert(shoesUid.Value, container);

        if (component.ChompSound != null)
            _audio.PlayPvs(component.ChompSound, uid);

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            if (_mind.TryGetObjectiveComp<CollectShoesConditionComponent>(mindId, out var cond, mind))
            {
                if (cond.Required <= 0)
                {
                    var targetReq = (int) Math.Ceiling(cond.Base + cond.PerPlayer * GetActivePlayers());
                    if (targetReq < cond.Min) targetReq = cond.Min;
                    if (targetReq > cond.Max) targetReq = cond.Max;
                    cond.Required = targetReq;
                }

                cond.Collected += 1;
            }

        _popup.PopupEntity(Loc.GetString("shoes-stolen-target-event"), target);
        _popup.PopupEntity(Loc.GetString("steal-shoes-event", ("shoes", MetaData(target).EntityName)), uid);
    }

    private void OnMobStateChanged(EntityUid uid, StealShoesComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (_containers.TryGetContainer(uid, component.ContainerId, out var container))
        {
            var dropCoords = Transform(uid).Coordinates;
            var toDrop = new List<EntityUid>(container.ContainedEntities);
            foreach (var ent in toDrop)
            {
                _containers.Remove(ent, container);
                _xform.SetCoordinates(ent, dropCoords);
                var angle = _random.NextFloat(0f, MathF.Tau);
                var speed = _random.NextFloat(2.5f, 4.5f);
                var vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                if (TryComp<PhysicsComponent>(ent, out var phys))
                    _physics.SetLinearVelocity(ent, vel);
            }
        }

        _body.GibBody(uid);
    }
}
