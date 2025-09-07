// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Content.Goobstation.Common.FloorGoblin;
using Content.Goobstation.Shared.FloorGoblin;
using Content.Shared._DV.Abilities;
using Content.Shared.Actions;
using Content.Shared.Climbing.Components;
using Content.Shared.Climbing.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

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
        SubscribeLocalEvent<CrawlUnderFloorComponent, AttackAttemptEvent>(OnAttemptAttack);
        SubscribeLocalEvent<AttackAttemptEvent>(OnAnyAttackAttempt);
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
        UpdateSneakCollision(uid, component);
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
        if (component.Enabled)
            UpdateSneakCollision(uid, component);
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
            if (_transform.GetGrid(xform.Coordinates) is not { } g || g != gridUid)
                continue;
            var now = IsOnSubfloor(uid);
            var old = _lastOnSubfloor.TryGetValue(uid, out var o) ? o : now;
            _lastOnSubfloor[uid] = now;
            if (comp.Enabled && now != old)
                UpdateSneakCollision(uid, comp);
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
        var old = _lastOnSubfloor.TryGetValue(uid, out var o) ? o : now;
        _lastOnSubfloor[uid] = now;
        if (comp.Enabled && now != old)
            UpdateSneakCollision(uid, comp);
        if (!old && now && comp.Enabled && _random.Prob(0.3f))
        {
            var idx = _random.Next(1, 8);
            var path = $"/Audio/_Goobstation/FloorGoblin/duende-0{idx}.ogg";
            _audio.PlayPvs(new SoundPathSpecifier(path), uid);
        }
    }

    private static int HiddenMask(int baseMask)
    => baseMask
       & (int) ~CollisionGroup.HighImpassable
       & (int) ~CollisionGroup.MidImpassable
       & (int) ~CollisionGroup.LowImpassable
       & (int) ~CollisionGroup.InteractImpassable;

    private static int HiddenLayer(int baseLayer)
        => baseLayer
           & (int) ~CollisionGroup.HighImpassable
           & (int) ~CollisionGroup.MidImpassable
           & (int) ~CollisionGroup.LowImpassable
           & (int) ~CollisionGroup.MobLayer;

    private static int GetOrCacheBase<TKey>(List<(TKey, int)> list, TKey key, int current)
    {
        var idx = list.FindIndex(t => EqualityComparer<TKey>.Default.Equals(t.Item1, key));
        if (idx >= 0)
            return list[idx].Item2;
        list.Add((key, current));
        return current;
    }

    private void UpdateSneakCollision(EntityUid uid, CrawlUnderFloorComponent comp)
    {
        if (!TryComp(uid, out FixturesComponent? fixtures))
            return;

        var hidden = IsHidden(uid, comp);

        foreach (var (key, fixture) in fixtures.Fixtures)
        {
            var baseMask = GetOrCacheBase(comp.ChangedFixtures, key, fixture.CollisionMask);
            var desiredMask = hidden ? HiddenMask(baseMask) : baseMask;
            if (fixture.CollisionMask != desiredMask)
                _physics.SetCollisionMask(uid, key, fixture, desiredMask, manager: fixtures);

            var baseLayer = GetOrCacheBase(comp.ChangedFixtureLayers, key, fixture.CollisionLayer);
            var desiredLayer = hidden ? HiddenLayer(baseLayer) : baseLayer;
            if (fixture.CollisionLayer != desiredLayer)
                _physics.SetCollisionLayer(uid, key, fixture, desiredLayer, manager: fixtures);
        }
    }

    private void OnAttemptAttack(EntityUid uid, CrawlUnderFloorComponent comp, AttackAttemptEvent args)
    {
        if (IsHidden(uid, comp))
            args.Cancel();
    }

    private void OnAnyAttackAttempt(AttackAttemptEvent args)
    {
        if (args.Target is not { } target)
            return;
        if (TryComp(target, out CrawlUnderFloorComponent? goblinComp) && IsHidden(target, goblinComp))
            args.Cancel();
    }
}
