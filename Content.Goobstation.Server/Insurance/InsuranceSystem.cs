using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Shared.Destructible;
using Content.Shared.Maps;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Content.Shared.Physics;
using Content.Shared.Random.Helpers;
using Robust.Shared.Physics.Components;

namespace Content.Goobstation.Server.Insurance;

public sealed partial class InsuranceSystem : EntitySystem
{
    [Dependency] private readonly SpawnOnDespawnSystem _spawnOnDespawn = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InsuranceComponent, DestructionEventArgs>(OnDestruct);
    }

    private void OnDestruct(Entity<InsuranceComponent> ent, ref DestructionEventArgs args)
    {
        if (!Exists(ent.Comp.PolicyOwner))
            return;

        if (!TryComp(ent.Comp.PolicyOwner, out TransformComponent? xform))
            return;

        var dropPod = Spawn("SpawnSupplyEmpty", SelectDropPos(xform, 2));
        var spawnOnDespawn = AddComp<SpawnOnDespawnComponent>(dropPod);
        _spawnOnDespawn.SetPrototypes(new Entity<SpawnOnDespawnComponent>(dropPod, spawnOnDespawn),
                ent.Comp.CompensationItems);
    }

    private MapCoordinates SelectDropPos(TransformComponent xform, int searchRadius)
    {
        if (xform.GridUid == null || !TryComp<MapGridComponent>(xform.GridUid.Value, out var grid))
            return _xform.ToMapCoordinates(xform.Coordinates);

        List<Vector2i> validTiles = [];

        var tileCoords = _map.CoordinatesToTile(xform.GridUid.Value, grid, xform.Coordinates);

        var physQuery = GetEntityQuery<PhysicsComponent>();

        for (int x = tileCoords.X - searchRadius; x <= tileCoords.X + searchRadius; x++)
        {
            for (int y = tileCoords.Y - searchRadius; y <= tileCoords.Y + searchRadius; y++)
            {
                Vector2i coords = new(x, y);

                if (!_map.TryGetTileRef(xform.GridUid.Value, grid, coords, out var tileRef) ||
                        _turf.IsSpace(tileRef))
                    continue;

                var valid = true;

                foreach (var ent in _map.GetAnchoredEntities(xform.GridUid.Value, grid, coords))
                {
                    if (!physQuery.TryGetComponent(ent, out var body))
                        continue;

                    if (body.Hard && (body.CollisionMask & (int) CollisionGroup.MobMask) != 0)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                    validTiles.Add(coords);
            }
        }

        if (validTiles.Count > 0)
            return _map.GridTileToWorld(xform.GridUid.Value, grid, _random.Pick(validTiles));
        else
            return _xform.ToMapCoordinates(xform.Coordinates);
    }
}
