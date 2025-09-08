// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.FloorGoblin;
using Content.Shared._DV.Abilities;
using Content.Shared.Actions;
using Content.Shared.Climbing.Components;
using Content.Shared.Climbing.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.FloorGoblin;

public abstract class SharedCrawlUnderFloorSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ITileDefinitionManager _tileManager = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
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
        if (_net.IsServer)
        {
            if (component.ToggleHideAction == null)
                _actionsSystem.AddAction(uid, ref component.ToggleHideAction, component.ActionProto);
            _lastOnSubfloor[uid] = IsOnSubfloor(uid);
        }
    }

    private void ToggledAbility(EntityUid uid, bool enabled)
    {
    }

    private void OnAbilityToggle(EntityUid uid, CrawlUnderFloorComponent component, ToggleCrawlingStateEvent args)
    {
        if (args.Handled)
            return;

        if (_net.IsClient)
        {
            var next = !component.Enabled;
            _appearance.SetData(uid, SneakMode.Enabled, next);
            args.Handled = true;
            return;
        }

        var result = component.Enabled ? DisableSneakMode(uid, component) : EnableSneakMode(uid, component);
        _appearance.SetData(uid, SneakMode.Enabled, component.Enabled);
        Dirty(uid, component);

        var now = IsOnSubfloor(uid);
        var old = _lastOnSubfloor.TryGetValue(uid, out var o) ? o : now;
        _lastOnSubfloor[uid] = now;

        if (component.Enabled && now != old)
            UpdateSneakCollision(uid, component);

        HandleCrawlTransition(uid, old, now, component, false);

        var wentUnder = component.Enabled && !IsOnSubfloor(uid);
        var selfKey = wentUnder ? "crawl-under-floor-toggle-on-self" : "crawl-under-floor-toggle-off-self";
        var othersKey = wentUnder ? "crawl-under-floor-toggle-on" : "crawl-under-floor-toggle-off";
        var name = MetaData(uid).EntityName;
        _popup.PopupEntity(Loc.GetString(selfKey), uid, uid);
        _popup.PopupEntity(Loc.GetString(othersKey, ("name", name)), uid, Filter.PvsExcept(uid), true, PopupType.Medium);

        args.Handled = result;
    }

    private void OnAttemptClimb(EntityUid uid, CrawlUnderFloorComponent component, AttemptClimbEvent args)
    {
        if (component.Enabled)
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

            HandleCrawlTransition(uid, old, now, comp, true);
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

        HandleCrawlTransition(uid, old, now, comp, false);
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

    protected void PlayDuendeSound(EntityUid uid, float probability = 0.3f)
    {
        if (!_net.IsServer)
            return;

        if (_random.Prob(probability))
        {
            var idx = _random.Next(1, 8);
            var path = $"/Audio/_Goobstation/FloorGoblin/duende-0{idx}.ogg";
            _audio.PlayPvs(new SoundPathSpecifier(path), uid);
        }
    }

    protected bool EnableSneakMode(EntityUid uid, CrawlUnderFloorComponent component)
    {
        if (component.Enabled || (TryComp<ClimbingComponent>(uid, out var climbing) && climbing.IsClimbing))
            return false;
        component.Enabled = true;
        Dirty(uid, component);
        UpdateSneakCollision(uid, component);
        return true;
    }

    protected bool DisableSneakMode(EntityUid uid, CrawlUnderFloorComponent component)
    {
        if (!component.Enabled || IsOnCollidingTile(uid) || (TryComp<ClimbingComponent>(uid, out var climbing) && climbing.IsClimbing))
            return false;
        component.Enabled = false;
        Dirty(uid, component);
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

    protected void UpdateSneakCollision(EntityUid uid, CrawlUnderFloorComponent comp)
    {
        if (!_net.IsServer)
            return;
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

    public bool IsOnCollidingTile(EntityUid uid)
    {
        var xform = Transform(uid);
        if (xform.Coordinates.GetTileRef() is not { } tile)
            return false;
        return _turf.IsTileBlocked(tile, CollisionGroup.SmallMobMask);
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

    public bool IsHidden(EntityUid uid, CrawlUnderFloorComponent comp)
        => comp.Enabled && !IsOnSubfloor(uid);

    private void HandleCrawlTransition(EntityUid uid, bool wasOnSubfloor, bool isOnSubfloor, CrawlUnderFloorComponent comp, bool causedByTileChange)
    {
        if (!_net.IsServer)
            return;
        if (!comp.Enabled)
            return;
        if (wasOnSubfloor == isOnSubfloor)
            return;

        var movedOutOfCover = !wasOnSubfloor && isOnSubfloor;

        if (movedOutOfCover)
            PlayDuendeSound(uid, causedByTileChange ? 1f : 0.3f);
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
}
