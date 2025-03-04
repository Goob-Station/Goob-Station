using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Server._Goobstation.Weapons.DelayedKnockdown;
using Content.Server.Body.Components;
using Content.Server.Temperature.Components;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Heretic;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.StatusEffect;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    public const string RustTile = "PlatingRust";

    private const float LeechingWalkUpdateInterval = 1f;
    private float _accumulator;

    public static readonly Dictionary<EntProtoId, EntProtoId> Transformations = new()
    {
        { "WallSolid", "WallSolidRust" },
        { "WallReinforced", "WallReinforcedRust" },
    };

    private void SubscribeRust()
    {
        SubscribeLocalEvent<HereticComponent, HereticLeechingWalkEvent>(OnLeechingWalk);
        SubscribeLocalEvent<HereticComponent, EventHereticRustConstruction>(OnRustConstruction);
        SubscribeLocalEvent<HereticComponent, EventHereticAggressiveSpread>(OnAggressiveSpread);

        SubscribeLocalEvent<SpriteRandomOffsetComponent, ComponentStartup>(OnRandomOffsetStartup);
    }

    private void OnRandomOffsetStartup(Entity<SpriteRandomOffsetComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        _appearance.SetData(uid,
            OffsetVisuals.Offset,
            _random.NextVector2Box(comp.MinX, comp.MinY, comp.MaxX, comp.MaxY));
    }

    private void OnAggressiveSpread(Entity<HereticComponent> ent, ref EventHereticAggressiveSpread args)
    {
        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;

        var multiplier = ent.Comp.CurrentPath == "Rust" ? MathF.Sqrt(ent.Comp.PathStage - 4) : 1f;
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

            foreach (var toRust in _lookup.GetEntitiesInRange(coords, args.LookupRange, LookupFlags.Static))
            {
                TryMakeRustWall(toRust);
            }
        }
    }

    public bool CanSurfaceBeRusted(EntityUid target, Entity<HereticComponent>? ent)
    {
        if (!TryComp(target, out RustRequiresPathStageComponent? requiresPathStage))
            return true;

        var stage = ent == null ? 10 : ent.Value.Comp.PathStage;

        if (requiresPathStage.PathStage <= stage)
            return true;

        if (ent != null)
            _popup.PopupEntity(Loc.GetString("heretic-ability-fail-rust-stage-low"), ent.Value, ent.Value);

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

    public bool TryMakeRustWall(EntityUid target, Entity<HereticComponent>? ent = null)
    {
        if (HasComp<RustedWallComponent>(target))
            return false;

        var proto = Prototype(target);

        var targetEntity = target;

        // Check transformations (walls into rusted walls)
        if (proto != null && Transformations.TryGetValue(proto.ID, out var transformation))
        {
            if (!CanSurfaceBeRusted(targetEntity, ent))
                return false;

            var xform = Transform(target);
            var rotation = xform.LocalRotation;
            var coords = _transform.GetMapCoordinates(target, xform);

            Del(target);

            targetEntity = Spawn(transformation, coords, rotation: rotation);
        }

        if (TerminatingOrDeleted(targetEntity) || !_tag.HasTag(targetEntity, "Wall"))
            return false;

        if (targetEntity == target && !CanSurfaceBeRusted(targetEntity, ent))
            return false;

        EnsureComp<RustedWallComponent>(targetEntity);

        var rune = EnsureComp<RustRuneComponent>(targetEntity);
        rune.RuneIndex = _random.Next(rune.RuneSprites.Count);
        rune.RuneOffset = _random.NextVector2Box(0.25f, 0.25f);
        // If targetEntity is target (which means no transformations were performed) - we add rust overlay
        rune.RustOverlay = targetEntity == target;
        Dirty(targetEntity, rune);

        return true;
    }

    private void OnRustConstruction(Entity<HereticComponent> ent, ref EventHereticRustConstruction args)
    {
        if (!TryUseAbility(ent, args))
            return;

        if (!IsTileRust(args.Target, out var pos))
        {
            _popup.PopupEntity(Loc.GetString("heretic-ability-fail-tile-not-rusted"), ent, ent);
            return;
        }

        var mask = CollisionGroup.LowImpassable | CollisionGroup.MidImpassable | CollisionGroup.HighImpassable |
                   CollisionGroup.Impassable;

        var lookup =
            _lookup.GetEntitiesInRange<FixturesComponent>(args.Target, args.ObstacleCheckRange, LookupFlags.Static);
        foreach (var (_, fix) in lookup)
        {
            if (fix.Fixtures.All(x => (x.Value.CollisionLayer & (int) mask) == 0))
                continue;

            _popup.PopupEntity(Loc.GetString("heretic-ability-fail-tile-occupied"), ent, ent);
            return;
        }

        var mapCoords = _transform.ToMapCoordinates(args.Target);

        var lookup2 =
            _lookup.GetEntitiesInRange<TransformComponent>(args.Target, args.MobCheckRange, LookupFlags.Dynamic);
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
        var rune = EnsureComp<RustRuneComponent>(wall);
        rune.RuneIndex = _random.Next(rune.RuneSprites.Count);
        rune.RuneOffset = _random.NextVector2Box(0.25f, 0.25f);
        Dirty(wall, rune);

        _aud.PlayPvs(args.Sound, args.Target);
    }

    private void OnLeechingWalk(Entity<HereticComponent> ent, ref HereticLeechingWalkEvent args)
    {
        EnsureComp<LeechingWalkComponent>(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulator += frameTime;

        if (_accumulator < LeechingWalkUpdateInterval)
            return;

        _accumulator = 0f;

        var damageableQuery = GetEntityQuery<DamageableComponent>();
        var bloodQuery = GetEntityQuery<BloodstreamComponent>();
        var solutionQuery = GetEntityQuery<SolutionContainerManagerComponent>();
        var temperatureQuery = GetEntityQuery<TemperatureComponent>();
        var staminaQuery = GetEntityQuery<StaminaComponent>();
        var statusQuery = GetEntityQuery<StatusEffectsComponent>();

        var query = EntityQueryEnumerator<LeechingWalkComponent, HereticComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var leech, out var heretic, out var xform))
        {
            if (!IsTileRust(xform.Coordinates, out _))
                continue;

            RemCompDeferred<DelayedKnockdownComponent>(uid);

            var multiplier = heretic.Ascended ? leech.AscensuionMultiplier : 1f;

            if (damageableQuery.TryComp(uid, out var damageable))
            {
                _dmg.TryChangeDamage(uid,
                    leech.ToHeal * multiplier,
                    true,
                    false,
                    damageable,
                    null,
                    false,
                    targetPart: TargetBodyPart.All);
            }

            if (bloodQuery.TryComp(uid, out var blood))
            {
                if (blood.BleedAmount > 0f)
                    _blood.TryModifyBleedAmount(uid, -blood.BleedAmount, blood);

                if (solutionQuery.TryComp(uid, out var sol) &&
                    _solution.ResolveSolution((uid, sol), blood.BloodSolutionName, ref blood.BloodSolution) &&
                    blood.BloodSolution.Value.Comp.Solution.Volume < blood.BloodMaxVolume)
                {
                    _blood.TryModifyBloodLevel(uid,
                        FixedPoint2.Min(leech.BloodHeal * multiplier,
                            blood.BloodMaxVolume - blood.BloodSolution.Value.Comp.Solution.Volume),
                        blood);
                }
            }

            if (temperatureQuery.TryComp(uid, out var temperature))
            {
                _temperature.ForceChangeTemperature(uid,
                    leech.TargetTemperature + (leech.TargetTemperature - temperature.CurrentTemperature) *
                    leech.AdjustTemperatureMultiplier * multiplier,
                    temperature);
            }

            if (staminaQuery.TryComp(uid, out var stamina) && stamina.StaminaDamage > 0)
            {
                _stam.TakeStaminaDamage(uid,
                    -float.Min(leech.StaminaHeal * multiplier, stamina.StaminaDamage),
                    stamina,
                    visual: false);
            }

            if (statusQuery.TryComp(uid, out var status))
            {
                var reduction = leech.StunReduction * multiplier;
                _statusEffect.TryRemoveTime(uid, "Stun", reduction, status);
                _statusEffect.TryRemoveTime(uid, "KnockedDown", reduction, status);
            }
        }
    }

    private bool IsTileRust(EntityCoordinates coords, [NotNullWhen(true)] out Vector2i? tileCoords)
    {
        tileCoords = null;
        if (!_mapMan.TryFindGridAt(_transform.ToMapCoordinates(coords), out var gridUid, out var mapGrid))
            return false;

        var tileRef = _map.GetTileRef(gridUid, mapGrid, coords);
        var tileDef = (ContentTileDefinition) _tileDefinitionManager[tileRef.Tile.TypeId];

        tileCoords = tileRef.GridIndices;
        return tileDef.ID == RustTile;
    }
}
