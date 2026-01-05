// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.SecondSkin;
using Content.Server.Heretic.Components.PathSpecific;
using Content.Server.Spreader;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Flash;
using Content.Shared.Heretic;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Tiles;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    public static readonly Dictionary<EntProtoId, EntProtoId> Transformations = new()
    {
        { "WallSolid", "WallSolidRust" },
        { "WallReinforced", "WallReinforcedRust" },
    };

    protected override void SubscribeRust()
    {
        base.SubscribeRust();

        SubscribeLocalEvent<HereticComponent, HereticLeechingWalkEvent>(OnLeechingWalk);
        SubscribeLocalEvent<HereticComponent, EventHereticRustConstruction>(OnRustConstruction);
        SubscribeLocalEvent<GhoulComponent, EventHereticAggressiveSpread>(OnGhoulAggressiveSpread);
        SubscribeLocalEvent<HereticComponent, EventHereticAggressiveSpread>(OnHereticAggressiveSpread);
        SubscribeLocalEvent<HereticComponent, EventHereticEntropicPlume>(OnEntropicPlume);
        SubscribeLocalEvent<HereticComponent, HereticAscensionRustEvent>(OnAscensionRust);

        SubscribeLocalEvent<SpriteRandomOffsetComponent, ComponentStartup>(OnRandomOffsetStartup);

        SubscribeLocalEvent<RustbringerComponent, FlashAttemptEvent>(OnFlashAttempt);

        SubscribeLocalEvent<LeechingWalkComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<LeechingWalkComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp(ent, out DisgustComponent? disgust))
            return;

        disgust.AccumulationMultiplier = 0f;
    }

    private void OnFlashAttempt(Entity<RustbringerComponent> ent, ref FlashAttemptEvent args)
    {
        if (!IsTileRust(Transform(ent).Coordinates, out _))
            return;

        args.Cancelled = true;
    }

    private void OnAscensionRust(Entity<HereticComponent> ent, ref HereticAscensionRustEvent args)
    {
        EnsureComp<LeechingWalkComponent>(ent); // Just in case
        EnsureComp<RustbringerComponent>(ent);
    }

    private void OnHereticAggressiveSpread(Entity<HereticComponent> ent, ref EventHereticAggressiveSpread args)
    {
        var effectiveStage = MathF.Max(ent.Comp.PathStage - 4f, 1f);
        var multiplier = ent.Comp.CurrentPath == "Rust" ? MathF.Sqrt(effectiveStage) : 1f;
        OnAggressiveSpread(ent, ref args, multiplier);
    }

    private void OnGhoulAggressiveSpread(Entity<GhoulComponent> ent, ref EventHereticAggressiveSpread args)
    {
        OnAggressiveSpread(ent, ref args, 2.2f);
    }

    private void OnEntropicPlume(Entity<HereticComponent> ent, ref EventHereticEntropicPlume args)
    {
        var uid = ent.Owner;

        if (!TryUseAbility(uid, args))
            return;

        args.Handled = true;

        var xform = Transform(uid);

        var (pos, rot) = _transform.GetWorldPositionRotation(xform);

        var dir = rot.ToWorldVec();

        var mapPos = new MapCoordinates(pos + dir * args.Offset, xform.MapID);

        var plume = Spawn(args.Proto, mapPos);

        RustObjectsInRadius(mapPos, args.Radius, args.TileRune, args.LookupRange, args.RustStrength);

        _gun.ShootProjectile(plume, dir, Vector2.Zero, uid, uid, args.Speed);
        _gun.SetTarget(plume, null, out _);
    }

    private void RustObjectsInRadius(MapCoordinates mapPos,
        float radius,
        string tileRune,
        float lookupRange,
        int rustStrength)
    {
        var circle = new Circle(mapPos.Position, radius);
        var grids = new List<Entity<MapGridComponent>>();
        var box = Box2.CenteredAround(mapPos.Position, new Vector2(radius, radius));
        _mapMan.FindGridsIntersecting(mapPos.MapId, box, ref grids);

        var tiles = new List<(EntityCoordinates, TileRef, EntityUid, MapGridComponent)>();
        foreach (var grid in grids)
        {
            tiles.AddRange(_map.GetTilesIntersecting(grid.Owner, grid.Comp, circle)
                .Select(x => (_map.GridTileToLocal(grid.Owner, grid.Comp, x.GridIndices), x, grid.Owner, grid.Comp)));
        }

        foreach (var (coords, tileRef, gridUid, mapGrid) in tiles)
        {
            if (CanRustTile((ContentTileDefinition) _tileDefinitionManager[tileRef.Tile.TypeId]))
                MakeRustTile(gridUid, mapGrid, tileRef, tileRune);

            foreach (var toRust in Lookup.GetEntitiesInRange(coords, lookupRange, LookupFlags.Static))
            {
                TryMakeRustWall(toRust, null, rustStrength);
            }
        }
    }

    private void OnRandomOffsetStartup(Entity<SpriteRandomOffsetComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        _appearance.SetData(uid,
            OffsetVisuals.Offset,
            _random.NextVector2Box(comp.MinX, comp.MinY, comp.MaxX, comp.MaxY));
    }

    private void OnAggressiveSpread(EntityUid ent, ref EventHereticAggressiveSpread args, float multiplier = 1f)
    {
        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;

        var aoeRadius = MathF.Max(args.AoeRadius, args.AoeRadius * multiplier);
        var range = MathF.Max(args.Range, args.Range * multiplier);

        var mapPos = _transform.GetMapCoordinates(args.Performer);
        var box = Box2.CenteredAround(mapPos.Position, new Vector2(range, range));
        var circle = new Circle(mapPos.Position, range);
        var grids = new List<Entity<MapGridComponent>>();
        _mapMan.FindGridsIntersecting(mapPos.MapId, box, ref grids);

        var tiles = new List<(EntityCoordinates, TileRef, EntityUid, MapGridComponent)>();
        foreach (var grid in grids)
        {
            tiles.AddRange(_map.GetTilesIntersecting(grid.Owner, grid.Comp, circle)
                .Select(x => (_map.GridTileToLocal(grid.Owner, grid.Comp, x.GridIndices), x, grid.Owner, grid.Comp)));
        }

        foreach (var (coords, tileRef, gridUid, mapGrid) in tiles)
        {
            var distanceToCaster = (_transform.ToMapCoordinates(coords).Position - mapPos.Position).Length();
            var chanceOfNotRusting = Math.Clamp((MathF.Max(distanceToCaster, 1f) - 1f) / (aoeRadius - 1f), 0f, 1f);

            if (_random.Prob(chanceOfNotRusting))
                continue;

            if (CanRustTile((ContentTileDefinition) _tileDefinitionManager[tileRef.Tile.TypeId]))
                MakeRustTile(gridUid, mapGrid, tileRef, args.TileRune);

            foreach (var toRust in Lookup.GetEntitiesInRange(coords, args.LookupRange, LookupFlags.Static))
            {
                TryMakeRustWall(toRust, null, args.RustStrength);
            }
        }
    }

    public bool CanSurfaceBeRusted(EntityUid target, Entity<HereticComponent>? ent, out int surfaceStrength)
    {
        surfaceStrength = 0;

        if (!TryComp(target, out RustRequiresPathStageComponent? requiresPathStage))
            return true;

        var stage = ent == null ? 10 : ent.Value.Comp.PathStage;
        surfaceStrength = requiresPathStage.PathStage;

        if (surfaceStrength <= stage)
            return true;

        if (ent != null)
            Popup.PopupEntity(Loc.GetString("heretic-ability-fail-rust-stage-low"), ent.Value, ent.Value);

        return false;
    }

    public bool CanRustTile(ContentTileDefinition tile)
    {
        return tile.ID != RustTile && !tile.Indestructible &&
               !(tile.DeconstructTools.Count == 0 && tile.Weather);
    }

    public void MakeRustTile(EntityUid gridUid, MapGridComponent mapGrid, TileRef tileRef, EntProtoId tileRune)
    {
        var plating = _tileDefinitionManager[RustTile];
        _map.SetTile(gridUid, mapGrid, tileRef.GridIndices, new Tile(plating.TileId));

        Spawn(tileRune, new EntityCoordinates(gridUid, tileRef.GridIndices));
    }

    public bool TryMakeRustWall(EntityUid target, Entity<HereticComponent>? ent = null, int? rustStrengthOverride = null)
    {
        var canRust = CanSurfaceBeRusted(target, ent, out var surfaceStrength);

        if (HasComp<RustedWallComponent>(target))
        {
            if (surfaceStrength > (rustStrengthOverride ?? ent?.Comp.PathStage ?? -1))
                return false;

            Del(target);
            return true;
        }

        var proto = Prototype(target);

        var targetEntity = target;

        // Check transformations (walls into rusted walls)
        if (proto != null && Transformations.TryGetValue(proto.ID, out var transformation))
        {
            if (!canRust)
                return false;

            var xform = Transform(target);
            var rotation = xform.LocalRotation;
            var coords = _transform.GetMapCoordinates(target, xform);

            Del(target);

            targetEntity = Spawn(transformation, coords, rotation: rotation);
        }

        if (TerminatingOrDeleted(targetEntity) || !_tag.HasTag(targetEntity, "Wall"))
            return false;

        if (targetEntity == target && !canRust)
            return false;

        EnsureComp<RustedWallComponent>(targetEntity);

        var rune = AddRustRune(targetEntity);
        // If targetEntity is target (which means no transformations were performed) - we add rust overlay
        rune.RustOverlay = targetEntity == target;

        return true;
    }

    private void OnRustConstruction(Entity<HereticComponent> ent, ref EventHereticRustConstruction args)
    {
        if (!TryUseAbility(ent, args))
            return;

        if (!IsTileRust(args.Target, out var pos))
        {
            Popup.PopupEntity(Loc.GetString("heretic-ability-fail-tile-not-rusted"), ent, ent);
            return;
        }

        var mask = CollisionGroup.LowImpassable | CollisionGroup.MidImpassable | CollisionGroup.HighImpassable |
                   CollisionGroup.Impassable;

        var lookup =
            Lookup.GetEntitiesInRange<FixturesComponent>(args.Target, args.ObstacleCheckRange, LookupFlags.Static);
        foreach (var (_, fix) in lookup)
        {
            if (fix.Fixtures.All(x => (x.Value.CollisionLayer & (int) mask) == 0))
                continue;

            Popup.PopupEntity(Loc.GetString("heretic-ability-fail-tile-occupied"), ent, ent);
            return;
        }

        var mapCoords = _transform.ToMapCoordinates(args.Target);

        var lookup2 =
            Lookup.GetEntitiesInRange<TransformComponent>(args.Target, args.MobCheckRange, LookupFlags.Dynamic);
        foreach (var (entity, xform) in lookup2)
        {
            var dir = _transform.GetWorldPosition(xform) - mapCoords.Position;
            if (dir.LengthSquared() < 0.001f)
                continue;
            _throw.TryThrow(entity, dir.Normalized() * args.ThrowRange, args.ThrowSpeed);
            _stun.KnockdownOrStun(entity, args.KnockdownTime, true);
            if (entity != args.Performer)
                _dmg.TryChangeDamage(entity, args.Damage, targetPart: TargetBodyPart.All);
        }

        args.Handled = true;
        RaiseNetworkEvent(new StopTargetingEvent(), args.Performer);

        var coords = new EntityCoordinates(args.Target.EntityId, pos.Value);
        var wall = Spawn(args.RustedWall, coords);
        AddRustRune(wall);

        _aud.PlayPvs(args.Sound, args.Target);
    }

    private RustRuneComponent AddRustRune(EntityUid wall)
    {
        var rune = EnsureComp<RustRuneComponent>(wall);
        rune.RuneIndex = _random.Next(rune.RuneSprites.Count);
        rune.RuneOffset = _random.NextVector2Box(0.25f, 0.25f);
        Dirty(wall, rune);

        Timer.Spawn(TimeSpan.FromSeconds(0.7f),
            () =>
            {
                if (TerminatingOrDeleted(wall) || !Resolve(wall, ref rune, false))
                    return;

                rune.AnimationEnded = true;
                Dirty(wall, rune);
            });

        return rune;
    }

    private void OnLeechingWalk(Entity<HereticComponent> ent, ref HereticLeechingWalkEvent args)
    {
        EnsureComp<LeechingWalkComponent>(ent);
    }
}
