// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.Abilities;
using Content.Shared._Starlight.VentCrawling;
using Content.Shared.Actions;
using Content.Shared.Climbing.Components;
using Content.Shared.Climbing.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.FloorGoblin;

public abstract class SharedCrawlUnderFloorSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ITileDefinitionManager _tileManager = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;

    private const int HiddenMask = (int) (CollisionGroup.HighImpassable | CollisionGroup.MidImpassable | CollisionGroup.LowImpassable | CollisionGroup.InteractImpassable);
    private const int HiddenLayer = (int) (CollisionGroup.HighImpassable | CollisionGroup.MidImpassable | CollisionGroup.LowImpassable | CollisionGroup.MobLayer);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrawlUnderFloorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CrawlUnderFloorComponent, ToggleCrawlingStateEvent>(OnAbilityToggle);
        SubscribeLocalEvent<CrawlUnderFloorComponent, AttemptClimbEvent>(OnAttemptClimb);
        SubscribeLocalEvent<MapGridComponent, TileChangedEvent>(OnTileChanged);
        SubscribeLocalEvent<CrawlUnderFloorComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<CrawlUnderFloorComponent, AttackAttemptEvent>(OnAttemptAttack);
        SubscribeLocalEvent<AttackAttemptEvent>(OnAnyAttackAttempt);
    }

    private void OnMapInit(EntityUid uid, CrawlUnderFloorComponent component, MapInitEvent args)
    {
        if (component.ToggleHideAction == null)
            _actionsSystem.AddAction(uid, ref component.ToggleHideAction, component.ActionProto);
        component.WasOnSubfloor = IsOnSubfloor(uid);

        if (!_net.IsClient)
        {
            EnableSneakMode(uid, component);
            SetStealth(uid, !IsOnSubfloor(uid)); // We use stealth component for allowing medhuds and such to be hidden, terrible solution, couldn't think of anything better.
        }
    }

    private void OnAbilityToggle(EntityUid uid, CrawlUnderFloorComponent component, ToggleCrawlingStateEvent args)
    {
        if (args.Handled)
            return;

        if (_net.IsClient)
        {
            args.Handled = true;
            return;
        }

        if (TryComp<VentCrawlerComponent>(uid, out var vent) && vent.InTube)
        {
            args.Handled = true;
            return;
        }

        var wasOnSubfloor = IsOnSubfloor(uid);
        var result = component.Enabled ? DisableSneakMode(uid, component) : EnableSneakMode(uid, component);

        RefreshCrawlSubfloorState(uid, component, false);

        var onSubfloorNow = IsOnSubfloor(uid);
        if (component.Enabled)
            SetStealth(uid, !onSubfloorNow);
        else
            SetStealth(uid, false);

        if (!wasOnSubfloor)
            PryTileIfUnder(uid, component);

        var enabling = component.Enabled;
        var selfKey = enabling ? "crawl-under-floor-toggle-on-self" : "crawl-under-floor-toggle-off-self";
        var othersKey = enabling ? "crawl-under-floor-toggle-on" : "crawl-under-floor-toggle-off";

        _popup.PopupEntity(Loc.GetString(selfKey), uid, uid);
        _popup.PopupEntity(Loc.GetString(othersKey, ("name", Name(uid))), uid, Filter.PvsExcept(uid), true, PopupType.Medium);

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

            ProcessCrawlStateChange(uid, comp, true);
        }
    }

    private void OnMove(EntityUid uid, CrawlUnderFloorComponent comp, ref MoveEvent args)
    {
        ProcessCrawlStateChange(uid, comp, false);
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
        if (_random.Prob(probability))
        {
            _audio.PlayPvs(new SoundCollectionSpecifier("DuendeSounds"), uid);
        }
    }

    protected bool EnableSneakMode(EntityUid uid, CrawlUnderFloorComponent component)
    {
        if (TryComp<VentCrawlerComponent>(uid, out var vent) && vent.InTube)
            return false;
        if (component.Enabled || (TryComp<ClimbingComponent>(uid, out var climbing) && climbing.IsClimbing))
            return false;
        component.Enabled = true;
        Dirty(uid, component);
        UpdateSneakCollision(uid, component);
        return true;
    }

    protected bool DisableSneakMode(EntityUid uid, CrawlUnderFloorComponent component)
    {
        if (TryComp<VentCrawlerComponent>(uid, out var vent) && vent.InTube)
            return false;
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
        if (!TryComp(uid, out FixturesComponent? fixtures))
            return;

        var hidden = IsHidden(uid, comp);

        foreach (var (key, fixture) in fixtures.Fixtures)
        {
            var baseMask = GetOrCacheBase(comp.ChangedFixtures, key, fixture.CollisionMask);
            var desiredMask = hidden ? GetHiddenMask(baseMask) : baseMask;
            if (fixture.CollisionMask != desiredMask)
                _physics.SetCollisionMask(uid, key, fixture, desiredMask, manager: fixtures);

            var baseLayer = GetOrCacheBase(comp.ChangedFixtureLayers, key, fixture.CollisionLayer);
            var desiredLayer = hidden ? GetHiddenLayer(baseLayer) : baseLayer;
            if (fixture.CollisionLayer != desiredLayer)
                _physics.SetCollisionLayer(uid, key, fixture, desiredLayer, manager: fixtures);
        }
    }

    public bool IsOnCollidingTile(EntityUid uid)
    {
        if (!TryGetCurrentTile(uid, out var tileRef, out _))
            return false;
        if (tileRef.Tile.IsEmpty)
            return false;
        return _turf.IsTileBlocked(tileRef, CollisionGroup.MobMask);
    }

    public bool IsOnSubfloor(EntityUid uid)
    {
        if (!TryGetCurrentTile(uid, out var tileRef, out _))
            return false;
        if (tileRef.Tile.IsEmpty)
            return false;
        var tileDef = (ContentTileDefinition) _tileManager[tileRef.Tile.TypeId];
        return tileDef.IsSubFloor;
    }

    private bool IsInSpace(EntityUid uid)
    {
        if (!TryGetCurrentTile(uid, out var tileRef, out _))
            return true;
        return tileRef.Tile.IsEmpty;
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
        var enteredCover = wasOnSubfloor && !isOnSubfloor;

        if (enteredCover)
            SetStealth(uid, true);
        else if (movedOutOfCover)
            SetStealth(uid, false);

        if (movedOutOfCover)
            PlayDuendeSound(uid, causedByTileChange ? 1f : 0.3f);
    }


    private static int GetHiddenMask(int baseMask)
        => baseMask
           & ~HiddenMask;

    private static int GetHiddenLayer(int baseLayer)
        => baseLayer
           & ~HiddenLayer;

    private static int GetOrCacheBase<TKey>(List<(TKey, int)> list, TKey key, int current)
    {
        var idx = list.FindIndex(t => EqualityComparer<TKey>.Default.Equals(t.Item1, key));
        if (idx >= 0)
            return list[idx].Item2;
        list.Add((key, current));
        return current;
    }

    private void PryTileIfUnder(EntityUid uid, CrawlUnderFloorComponent comp)
    {
        if (!TryGetCurrentTile(uid, out var tileRef, out var snapPos))
            return;
        if (tileRef.Tile.IsEmpty || ((ContentTileDefinition) _tileManager[tileRef.Tile.TypeId]).IsSubFloor)
            return;

        var coords = Transform(uid).Coordinates;
        if (_transform.GetGrid(coords) is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out _))
            return;

        _audio.PlayPvs(comp.PrySound, uid);

        _tile.PryTile(snapPos, gridUid);
    }

    private void SetStealth(EntityUid uid, bool enabled)
    {
        // Evil hud overlay hiding shitcode that hijacks StealthComponent
        if (enabled)
        {
            var stealth = EnsureComp<StealthComponent>(uid);
            if (!stealth.Enabled)
            {
                _stealth.SetEnabled(uid, true);
                Dirty(uid, stealth);
            }
        }
        else
        {
            if (TryComp<StealthComponent>(uid, out var stealth) && stealth.Enabled)
            {
                _stealth.SetEnabled(uid, false);
                Dirty(uid, stealth);
            }
        }
    }



    private void RefreshCrawlSubfloorState(EntityUid uid, CrawlUnderFloorComponent comp, bool causedByTileChange)
    {
        var now = IsOnSubfloor(uid);
        var old = comp.WasOnSubfloor;
        comp.WasOnSubfloor = now;

        if (comp.Enabled && now != old)
            UpdateSneakCollision(uid, comp);

        HandleCrawlTransition(uid, old, now, comp, causedByTileChange);
    }

    private void ProcessCrawlStateChange(EntityUid uid, CrawlUnderFloorComponent comp, bool causedByTileChange)
    {
        if (comp.Enabled && IsInSpace(uid))
        {
            DisableSneakMode(uid, comp);
            return;
        }

        RefreshCrawlSubfloorState(uid, comp, causedByTileChange);
    }

    private bool TryGetCurrentTile(EntityUid uid, out TileRef tileRef, out Vector2i snapPos)
    {
        var transform = Transform(uid);
        tileRef = default;
        snapPos = default;
        if (_transform.GetGrid(transform.Coordinates) is not { } gridUid)
            return false;
        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return false;
        snapPos = _map.TileIndicesFor((gridUid, grid), transform.Coordinates);
        tileRef = _map.GetTileRef(gridUid, grid, snapPos);
        return true;
    }
}
