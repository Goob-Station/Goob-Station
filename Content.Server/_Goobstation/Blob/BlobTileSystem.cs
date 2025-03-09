using System.Linq;
using System.Numerics;
using Content.Server.Construction.Components;
using Content.Server.Destructible;
using Content.Server.Emp;
using Content.Shared._Goobstation.Blob;
using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Blob;

public sealed class BlobTileSystem : SharedBlobTileSystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly BlobCoreSystem _blobCoreSystem = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly EmpSystem _empSystem = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly NpcFactionSystem _npcFactionSystem = default!;

    private EntityQuery<BlobCoreComponent> _blobCoreQuery;

    [ValidatePrototypeId<NpcFactionPrototype>]
    private const string BlobFaction = "Blob";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobTileComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BlobTileComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<BlobTileComponent, BlobTileGetPulseEvent>(OnPulsed);
        SubscribeLocalEvent<BlobTileComponent, EntityTerminatingEvent>(OnTerminate);

        _blobCoreQuery = GetEntityQuery<BlobCoreComponent>();
    }

    private void OnMapInit(Entity<BlobTileComponent> ent, ref MapInitEvent args)
    {
        var faction = EnsureComp<NpcFactionMemberComponent>(ent);
        Entity<NpcFactionMemberComponent?> factionEnt = (ent, faction);

        _npcFactionSystem.ClearFactions(factionEnt, false);
        _npcFactionSystem.AddFaction(factionEnt, BlobFaction, true);

        // make alive - true for npc combat
        EnsureComp<MobStateComponent>(ent);
    }

    private void OnTerminate(EntityUid uid, BlobTileComponent component, EntityTerminatingEvent args)
    {
        if (TerminatingOrDeleted(component.Core))
            return;

        component.Core!.Value.Comp.BlobTiles.Remove(uid);
    }

    private void OnDestruction(EntityUid uid, BlobTileComponent component, DestructionEventArgs args)
    {
        if (
            TerminatingOrDeleted(component.Core) ||
            !_blobCoreQuery.TryComp(component.Core, out var blobCoreComponent)
            )
            return;

        if (blobCoreComponent.CurrentChem == BlobChemType.ElectromagneticWeb)
        {
            _empSystem.EmpPulse(_transform.GetMapCoordinates(uid), 3f, 50f, 3f);
        }
    }

    private void OnPulsed(EntityUid uid, BlobTileComponent component, BlobTileGetPulseEvent args)
    {
        if (component.Core == null)
            return;

        var core = component.Core.Value;

        if (core.Comp.CurrentChem == BlobChemType.RegenerativeMateria)
        {
            var healCore = new DamageSpecifier();
            foreach (var keyValuePair in component.HealthOfPulse.DamageDict)
            {
                healCore.DamageDict.Add(keyValuePair.Key, keyValuePair.Value * 5);
            }

            _damageableSystem.TryChangeDamage(uid, healCore);
        }
        else
        {
            _damageableSystem.TryChangeDamage(uid, component.HealthOfPulse);
        }

        if (!args.Handled)
            return;

        var xform = Transform(uid);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
        {
            return;
        }

        var nearNode = _blobCoreSystem.GetNearNode(xform.Coordinates, core.Comp.TilesRadiusLimit);

        if (nearNode == null)
            return;

        var mobTile = _mapSystem.GetTileRef(xform.GridUid.Value, grid, xform.Coordinates);

        var mobAdjacentTiles = new[]
        {
            mobTile.GridIndices.Offset(Direction.East),
            mobTile.GridIndices.Offset(Direction.West),
            mobTile.GridIndices.Offset(Direction.North),
            mobTile.GridIndices.Offset(Direction.South),
        };

        _random.Shuffle(mobAdjacentTiles);

        var localPos = xform.Coordinates.Position;

        var radius = 1.0f;

        var innerTiles = _mapSystem.GetLocalTilesIntersecting(xform.GridUid.Value,
                grid,
                new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius)))
            .ToArray();

        foreach (var innerTile in innerTiles)
        {
            if (!mobAdjacentTiles.Contains(innerTile.GridIndices))
            {
                continue;
            }

            foreach (var ent in _mapSystem.GetAnchoredEntities(xform.GridUid.Value, grid, innerTile.GridIndices))
            {
                if (!HasComp<DestructibleComponent>(ent) || !HasComp<ConstructionComponent>(ent))
                    continue;

                DoLunge(uid, ent);
                _damageableSystem.TryChangeDamage(ent, core.Comp.ChemDamageDict[core.Comp.CurrentChem]);
                _audioSystem.PlayPvs(core.Comp.AttackSound, uid, AudioParams.Default);
                args.Handled = true;
                return;
            }

            var spawn = true;
            foreach (var ent in _mapSystem.GetAnchoredEntities(xform.GridUid.Value, grid, innerTile.GridIndices))
            {
                if (!HasComp<BlobTileComponent>(ent))
                    continue;
                spawn = false;
                break;
            }

            if (!spawn)
                continue;

            var location = _mapSystem.ToCoordinates(xform.GridUid.Value, innerTile.GridIndices, grid);

            if (_blobCoreSystem.TransformBlobTile(null,
                    core,
                    nearNode,
                    BlobTileType.Normal,
                    location))
                return;
        }
    }

    protected override void TryUpgrade(Entity<BlobTileComponent> target, Entity<BlobCoreComponent> core, EntityUid observer)
    {
        var coords = Transform(target).Coordinates;

        if (target.Comp.BlobTileType == BlobTileType.Reflective)
            return;

        var nearNode = _blobCoreSystem.GetNearNode(coords, core.Comp.TilesRadiusLimit);
        if (nearNode == null)
            return;

        var ev = new BlobTransformTileActionEvent(
            performer: observer,
            target: coords,
            transformFrom: target.Comp.BlobTileType,
            tileType: BlobTileType.Invalid,
            requireNode: false);

        ev.TileType = ev.TransformFrom switch
        {
            BlobTileType.Normal => BlobTileType.Strong,
            BlobTileType.Strong => BlobTileType.Reflective,
            _ => BlobTileType.Invalid
        };

        RaiseLocalEvent(core, ev);
    }

    /* This work very bad.
     I replace invisible
     wall to teleportation observer
     if he moving away from blob tile */

    // private void OnStartup(EntityUid uid, BlobCellComponent component, ComponentStartup args)
    // {
    //     var xform = Transform(uid);
    //     var radius = 2.5f;
    //     var wallSpacing = 1.5f; // Расстояние между стенами и центральной областью
    //
    //     if (!_map.TryGetGrid(xform.GridUid, out var grid))
    //     {
    //         return;
    //     }
    //
    //     var localpos = xform.Coordinates.Position;
    //
    //     // Получаем тайлы в области с радиусом 2.5
    //     var allTiles = grid.GetLocalTilesIntersecting(
    //         new Box2(localpos + new Vector2(-radius, -radius), localpos + new Vector2(radius, radius))).ToArray();
    //
    //     // Получаем тайлы в области с радиусом 1.5
    //     var innerTiles = grid.GetLocalTilesIntersecting(
    //         new Box2(localpos + new Vector2(-wallSpacing, -wallSpacing), localpos + new Vector2(wallSpacing, wallSpacing))).ToArray();
    //
    //     foreach (var tileref in innerTiles)
    //     {
    //         foreach (var ent in grid.GetAnchoredEntities(tileref.GridIndices))
    //         {
    //             if (HasComp<BlobBorderComponent>(ent))
    //                 QueueDel(ent);
    //             if (HasComp<BlobCellComponent>(ent))
    //             {
    //                 var blockTiles = grid.GetLocalTilesIntersecting(
    //                     new Box2(Transform(ent).Coordinates.Position + new Vector2(-wallSpacing, -wallSpacing),
    //                         Transform(ent).Coordinates.Position + new Vector2(wallSpacing, wallSpacing))).ToArray();
    //                 allTiles = allTiles.Except(blockTiles).ToArray();
    //             }
    //         }
    //     }
    //
    //     var outerTiles = allTiles.Except(innerTiles).ToArray();
    //
    //     foreach (var tileRef in outerTiles)
    //     {
    //         foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
    //         {
    //             if (HasComp<BlobCellComponent>(ent))
    //             {
    //                 var blockTiles = grid.GetLocalTilesIntersecting(
    //                     new Box2(Transform(ent).Coordinates.Position + new Vector2(-wallSpacing, -wallSpacing),
    //                         Transform(ent).Coordinates.Position + new Vector2(wallSpacing, wallSpacing))).ToArray();
    //                 outerTiles = outerTiles.Except(blockTiles).ToArray();
    //             }
    //         }
    //     }
    //
    //     foreach (var tileRef in outerTiles)
    //     {
    //         var spawn = true;
    //         foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
    //         {
    //             if (HasComp<BlobBorderComponent>(ent))
    //             {
    //                 spawn = false;
    //                 break;
    //             }
    //         }
    //         if (spawn)
    //             EntityManager.SpawnEntity("BlobBorder", tileRef.GridIndices.ToEntityCoordinates(xform.GridUid.Value, _map));
    //     }
    // }

    // private void OnDestruction(EntityUid uid, BlobTileComponent component, DestructionEventArgs args)
    // {
    //     var xform = Transform(uid);
    //     var radius = 1.0f;
    //
    //     if (!_map.TryGetGrid(xform.GridUid, out var grid))
    //     {
    //         return;
    //     }
    //
    //     var localPos = xform.Coordinates.Position;
    //
    //     var innerTiles = grid.GetLocalTilesIntersecting(
    //         new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius)), false).ToArray();
    //
    //     var centerTile = grid.GetLocalTilesIntersecting(
    //         new Box2(localPos, localPos)).ToArray();
    //
    //     innerTiles = innerTiles.Except(centerTile).ToArray();
    //
    //     foreach (var tileref in innerTiles)
    //     {
    //         foreach (var ent in grid.GetAnchoredEntities(tileref.GridIndices))
    //         {
    //             if (!HasComp<BlobTileComponent>(ent))
    //                 continue;
    //             var blockTiles = grid.GetLocalTilesIntersecting(
    //                 new Box2(Transform(ent).Coordinates.Position + new Vector2(-radius, -radius),
    //                     Transform(ent).Coordinates.Position + new Vector2(radius, radius)), false).ToArray();
    //
    //             var tilesToRemove = new List<TileRef>();
    //
    //             foreach (var blockTile in blockTiles)
    //             {
    //                 tilesToRemove.Add(blockTile);
    //             }
    //
    //             innerTiles = innerTiles.Except(tilesToRemove).ToArray();
    //         }
    //     }
    //
    //     foreach (var tileRef in innerTiles)
    //     {
    //         foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
    //         {
    //             if (HasComp<BlobBorderComponent>(ent))
    //             {
    //                 QueueDel(ent);
    //             }
    //         }
    //     }
    //
    //     EntityManager.SpawnEntity(component.BlobBorder, xform.Coordinates);
    // }
    //
    // private void OnStartup(EntityUid uid, BlobTileComponent component, ComponentStartup args)
    // {
    //     var xform = Transform(uid);
    //     var wallSpacing = 1.0f;
    //
    //     if (!_map.TryGetGrid(xform.GridUid, out var grid))
    //     {
    //         return;
    //     }
    //
    //     var localPos = xform.Coordinates.Position;
    //
    //     var innerTiles = grid.GetLocalTilesIntersecting(
    //         new Box2(localPos + new Vector2(-wallSpacing, -wallSpacing), localPos + new Vector2(wallSpacing, wallSpacing)), false).ToArray();
    //
    //     var centerTile = grid.GetLocalTilesIntersecting(
    //         new Box2(localPos, localPos)).ToArray();
    //
    //     foreach (var tileRef in centerTile)
    //     {
    //         foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
    //         {
    //             if (HasComp<BlobBorderComponent>(ent))
    //                 QueueDel(ent);
    //         }
    //     }
    //     innerTiles = innerTiles.Except(centerTile).ToArray();
    //
    //     foreach (var tileref in innerTiles)
    //     {
    //         var spaceNear = false;
    //         var hasBlobTile = false;
    //         foreach (var ent in grid.GetAnchoredEntities(tileref.GridIndices))
    //         {
    //             if (!HasComp<BlobTileComponent>(ent))
    //                 continue;
    //             var blockTiles = grid.GetLocalTilesIntersecting(
    //                 new Box2(Transform(ent).Coordinates.Position + new Vector2(-wallSpacing, -wallSpacing),
    //                     Transform(ent).Coordinates.Position + new Vector2(wallSpacing, wallSpacing)), false).ToArray();
    //
    //             var tilesToRemove = new List<TileRef>();
    //
    //             foreach (var blockTile in blockTiles)
    //             {
    //                 if (blockTile.Tile.IsEmpty)
    //                 {
    //                     spaceNear = true;
    //                 }
    //                 else
    //                 {
    //                     tilesToRemove.Add(blockTile);
    //                 }
    //             }
    //
    //             innerTiles = innerTiles.Except(tilesToRemove).ToArray();
    //
    //             hasBlobTile = true;
    //         }
    //
    //         if (!hasBlobTile || spaceNear)
    //             continue;
    //         {
    //             foreach (var ent in grid.GetAnchoredEntities(tileref.GridIndices))
    //             {
    //                 if (HasComp<BlobBorderComponent>(ent))
    //                 {
    //                     QueueDel(ent);
    //                 }
    //             }
    //         }
    //     }
    //
    //     var spaceNearCenter = false;
    //
    //     foreach (var tileRef in innerTiles)
    //     {
    //         var spawn = true;
    //         if (tileRef.Tile.IsEmpty)
    //         {
    //             spaceNearCenter = true;
    //             spawn = false;
    //         }
    //         if (grid.GetAnchoredEntities(tileRef.GridIndices).Any(ent => HasComp<BlobBorderComponent>(ent)))
    //         {
    //             spawn = false;
    //         }
    //         if (spawn)
    //             EntityManager.SpawnEntity(component.BlobBorder, tileRef.GridIndices.ToEntityCoordinates(xform.GridUid.Value, _map));
    //     }
    //     if (spaceNearCenter)
    //     {
    //         EntityManager.SpawnEntity(component.BlobBorder, xform.Coordinates);
    //     }
    // }
    public void SwapSpecials(Entity<BlobNodeComponent> from, Entity<BlobNodeComponent> to)
    {
        (from.Comp.BlobFactory, to.Comp.BlobFactory) = (to.Comp.BlobFactory, from.Comp.BlobFactory);
        (from.Comp.BlobResource, to.Comp.BlobResource) = (to.Comp.BlobResource, from.Comp.BlobResource);
        Dirty(from);
        Dirty(to);
    }

    public bool IsEmptySpecial(Entity<BlobNodeComponent> node, BlobTileType tile)
    {
        return tile switch
        {
            BlobTileType.Factory => node.Comp.BlobFactory == null || TerminatingOrDeleted(node.Comp.BlobFactory),
            BlobTileType.Resource => node.Comp.BlobResource == null || TerminatingOrDeleted(node.Comp.BlobResource),
            _ => false
        };
    }

    public void DoLunge(EntityUid from, EntityUid target)
    {
        if(!TransformQuery.TryComp(from, out var userXform))
            return;

        var targetPos = _transform.GetWorldPosition(target);
        var localPos = Vector2.Transform(targetPos, _transform.GetInvWorldMatrix(userXform));
        localPos = userXform.LocalRotation.RotateVec(localPos);

        RaiseNetworkEvent(new BlobAttackEvent(GetNetEntity(from), GetNetEntity(target), localPos), Filter.Pvs(from));
    }
}
