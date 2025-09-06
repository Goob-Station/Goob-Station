// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 portfiend <109661617+portfiend@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Content.Shared.Actions;
using Content.Shared.Climbing.Components;
using Content.Shared.Climbing.Events;
using Content.Goobstation.Shared.FloorGoblin;
using Content.Shared.Maps;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Content.Goobstation.Common.FloorGoblin;
using Content.Shared._DV.Abilities;

namespace Content.Goobstation.Server.FloorGoblin;

public sealed partial class CrawlUnderFloorSystem : SharedCrawlUnderFloorSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ITileDefinitionManager _tileManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly Dictionary<EntityUid, bool> _lastOnSubfloor = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrawlUnderFloorComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CrawlUnderFloorComponent, ToggleCrawlingStateEvent>(OnAbilityToggle);
        SubscribeLocalEvent<CrawlUnderFloorComponent, AttemptClimbEvent>(OnAttemptClimb);
        SubscribeLocalEvent<MapGridComponent, TileChangedEvent>(OnTileChanged);
        SubscribeLocalEvent<TransformComponent, MoveEvent>(OnMove);
    }

    private bool IsOnCollidingTile(EntityUid uid)
    {
        var xform = Transform(uid);
        var tile = xform.Coordinates.GetTileRef();
        if (tile == null)
            return false;
        return _turf.IsTileBlocked(tile.Value, CollisionGroup.SmallMobMask);
    }

    private bool IsOnSubfloor(EntityUid uid)
    {
        var xform = Transform(uid);
        var gridUid = _transform.GetGrid(xform.Coordinates);
        if (gridUid == null)
            return false;
        if (!TryComp<MapGridComponent>(gridUid.Value, out var grid))
            return false;
        var snapPos = _map.TileIndicesFor((gridUid.Value, grid), xform.Coordinates);
        var tileRef = _map.GetTileRef(gridUid.Value, grid, snapPos);
        if (tileRef.Tile.IsEmpty)
            return false;
        var tileDef = (ContentTileDefinition) _tileManager[tileRef.Tile.TypeId];
        return tileDef.IsSubFloor;
    }

    private void OnInit(EntityUid uid, CrawlUnderFloorComponent component, ComponentInit args)
    {
        if (component.ToggleHideAction == null)
            _actionsSystem.AddAction(uid, ref component.ToggleHideAction, component.ActionProto);
        _lastOnSubfloor[uid] = IsOnSubfloor(uid);
    }

    private bool EnableSneakMode(EntityUid uid, CrawlUnderFloorComponent component)
    {
        if (component.Enabled || (TryComp<ClimbingComponent>(uid, out var climbing) && climbing.IsClimbing == true))
            return false;

        component.Enabled = true;
        Dirty(uid, component);
        RaiseLocalEvent(uid, new CrawlingUpdatedEvent(component.Enabled));

        if (TryComp(uid, out FixturesComponent? fixtureComponent))
        {
            foreach (var (key, fixture) in fixtureComponent.Fixtures)
            {
                var newMask = fixture.CollisionMask
                    & (int) ~CollisionGroup.HighImpassable
                    & (int) ~CollisionGroup.MidImpassable
                    & (int) ~CollisionGroup.LowImpassable
                    & (int) ~CollisionGroup.InteractImpassable;
                if (fixture.CollisionMask != newMask)
                {
                    component.ChangedFixtures.Add((key, fixture.CollisionMask));
                    _physics.SetCollisionMask(uid, key, fixture, newMask, manager: fixtureComponent);
                }

                var newLayer = fixture.CollisionLayer
                    & (int) ~CollisionGroup.HighImpassable
                    & (int) ~CollisionGroup.MidImpassable
                    & (int) ~CollisionGroup.LowImpassable
                    & (int) ~CollisionGroup.BulletImpassable;
                if (fixture.CollisionLayer != newLayer)
                {
                    component.ChangedFixtureLayers.Add((key, fixture.CollisionLayer));
                    _physics.SetCollisionLayer(uid, key, fixture, newLayer, manager: fixtureComponent);
                }
            }
        }

        return true;
    }

    private bool DisableSneakMode(EntityUid uid, CrawlUnderFloorComponent component)
    {
        if (!component.Enabled || IsOnCollidingTile(uid) || (TryComp<ClimbingComponent>(uid, out var climbing) && climbing.IsClimbing == true))
            return false;

        component.Enabled = false;
        Dirty(uid, component);
        RaiseLocalEvent(uid, new CrawlingUpdatedEvent(component.Enabled));

        if (TryComp(uid, out FixturesComponent? fixtureComponent))
        {
            foreach (var (key, originalMask) in component.ChangedFixtures)
                if (fixtureComponent.Fixtures.TryGetValue(key, out var fixture))
                    _physics.SetCollisionMask(uid, key, fixture, originalMask, fixtureComponent);

            foreach (var (key, originalLayer) in component.ChangedFixtureLayers)
                if (fixtureComponent.Fixtures.TryGetValue(key, out var fixture))
                    _physics.SetCollisionLayer(uid, key, fixture, originalLayer, fixtureComponent);
        }

        component.ChangedFixtures.Clear();
        component.ChangedFixtureLayers.Clear();
        return true;
    }

    private void OnAbilityToggle(EntityUid uid, CrawlUnderFloorComponent component, ToggleCrawlingStateEvent args)
    {
        if (args.Handled)
            return;

        var result = component.Enabled ? DisableSneakMode(uid, component) : EnableSneakMode(uid, component);

        if (TryComp<AppearanceComponent>(uid, out var app))
            _appearance.SetData(uid, SneakMode.Enabled, component.Enabled, app);

        _lastOnSubfloor[uid] = IsOnSubfloor(uid);
        args.Handled = result;
    }

    private void OnAttemptClimb(EntityUid uid, CrawlUnderFloorComponent component, AttemptClimbEvent args)
    {
        if (component.Enabled == true)
            args.Cancelled = true;
    }

    private void OnTileChanged(EntityUid gridUid, MapGridComponent grid, ref TileChangedEvent args)
    {
        var query = EntityQueryEnumerator<CrawlUnderFloorComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            var g = _transform.GetGrid(xform.Coordinates);
            if (g == null || g != gridUid)
                continue;

            var now = IsOnSubfloor(uid);
            if (!_lastOnSubfloor.TryGetValue(uid, out var old))
                old = now;

            _lastOnSubfloor[uid] = now;

            if (!old && now && comp.Enabled && _random.Prob(0.3f))
            {
                var idx = _random.Next(1, 8);
                var path = $"/Audio/_Goobstation/FloorGoblin/duende-0{idx}.ogg";
                _audio.PlayPvs(new SoundPathSpecifier(path), uid);
            }
        }
    }

    private void OnMove(EntityUid uid, TransformComponent xform, ref MoveEvent args)
    {
        if (!TryComp<CrawlUnderFloorComponent>(uid, out var comp))
            return;

        var now = IsOnSubfloor(uid);
        if (!_lastOnSubfloor.TryGetValue(uid, out var old))
            old = now;

        _lastOnSubfloor[uid] = now;

        if (!old && now && comp.Enabled && _random.Prob(0.3f))
        {
            var idx = _random.Next(1, 8);
            var path = $"/Audio/_Goobstation/FloorGoblin/duende-0{idx}.ogg";
            _audio.PlayPvs(new SoundPathSpecifier(path), uid);
        }
    }
}
