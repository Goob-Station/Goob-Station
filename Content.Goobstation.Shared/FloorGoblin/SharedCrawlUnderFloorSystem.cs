// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.Abilities;
using Content.Shared._Starlight.VentCrawling;
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
using Content.Shared.Actions;
using Content.Shared.Mobs.Components;
using Content.Shared.Doors.Components;
using Content.Shared.Conveyor;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

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

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrawlUnderFloorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CrawlUnderFloorComponent, ToggleCrawlingStateEvent>(OnAbilityToggle);
        SubscribeLocalEvent<CrawlUnderFloorComponent, AttemptClimbEvent>(OnAttemptClimb);
        SubscribeLocalEvent<MapGridComponent, TileChangedEvent>(OnTileChanged);
        SubscribeLocalEvent<CrawlUnderFloorComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<CrawlUnderFloorComponent, PreventCollideEvent>(OnPreventCollision);
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
        // Just update the crawl state based on whether we're enabled
        ProcessCrawlStateChange(uid, comp, comp.Enabled);
    }


    private void OnAttemptAttack(EntityUid uid, CrawlUnderFloorComponent comp, AttackAttemptEvent args)
    {
        if (IsHidden(uid, comp))
            args.Cancel();
    }

    private void OnAnyAttackAttempt(AttackAttemptEvent ev)
    {
        if (HasComp<CrawlUnderFloorComponent>(ev.Target))
            ev.Cancel();
    }

    private void OnPreventCollision(EntityUid uid, CrawlUnderFloorComponent component, ref PreventCollideEvent args)
    {
        var otherUid = args.OtherEntity;

        // Always prevent collision with mobs
        if (HasComp<MobStateComponent>(otherUid))
        {
            args.Cancelled = true;
            return;
        }

        // Handle airlocks - allow phasing in stealth mode
        if (HasComp<AirlockComponent>(otherUid) && component.Enabled)
        {
            args.Cancelled = true;
            return;
        }

        // Handle conveyor belts - allow phasing in stealth mode
        if (HasComp<ConveyorComponent>(otherUid) && component.Enabled)
        {
            args.Cancelled = true;
            return;
        }
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


    public bool IsOnCollidingTile(EntityUid uid)
    {
        // If we're under the floor, don't consider any tiles as colliding
        if (TryComp<CrawlUnderFloorComponent>(uid, out var crawlComp) &&
            crawlComp.Enabled &&
            !IsOnSubfloor(uid))
        {
            return false;
        }

        // Standard collision check for tiles
        if (!TryGetCurrentTile(uid, out var tileRef, out _) || tileRef.Tile.IsEmpty)
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
        => comp.Enabled; // No longer check for subfloor, just check if crawling is enabled

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

    private void UpdateCollisionMask(EntityUid uid, bool stealthMode)
    {
        if (!TryComp<FixturesComponent>(uid, out var fixtures))
            return;

        if (stealthMode)
        {
            // In stealth mode, set to SmallMob collision to maintain some physics
            // while still allowing phasing through most objects
            foreach (var (id, fixture) in fixtures.Fixtures)
            {
                _physics.SetCollisionMask(uid, id, fixture, (int)CollisionGroup.SmallMobMask, fixtures);
                _physics.SetCollisionLayer(uid, id, fixture, (int)CollisionGroup.SmallMobLayer, fixtures);
            }
        }
        else
        {
            // In normal mode, use standard mob collision
            foreach (var (id, fixture) in fixtures.Fixtures)
            {
                _physics.SetCollisionMask(uid, id, fixture, (int)CollisionGroup.MobMask, fixtures);
                _physics.SetCollisionLayer(uid, id, fixture, (int)CollisionGroup.MobLayer, fixtures);
            }
        }

        Dirty(uid, fixtures);
    }

    private void SetStealth(EntityUid uid, bool enabled)
    {
        // Update collision mask based on stealth state
        UpdateCollisionMask(uid, enabled);

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
