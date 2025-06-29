using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared._Funkystation.Atmos.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._Funkystation.Atmos.EntitySystems;

[UsedImplicitly]
public sealed partial class AdjustAirOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly GasTileOverlaySystem _gasOverlaySystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AdjustAirOnTriggerComponent, TriggerEvent>(OnAdjustAirTrigger);
    }

    private void OnAdjustAirTrigger(EntityUid uid, AdjustAirOnTriggerComponent component, TriggerEvent args)
    {
        if (!_random.Prob(component.Probability))
            return;

        if (!TryComp<TransformComponent>(uid, out var xform))
            return;

        var coords = xform.Coordinates;
        if (!coords.IsValid(EntityManager))
            return;

        var mapCoords = _transformSystem.ToMapCoordinates(coords);
        if (mapCoords.MapId == MapId.Nullspace)
            return;

        if (!_mapManager.TryFindGridAt(mapCoords, out var gridUid, out var grid))
            return;

        var gridEntity = new Entity<GridAtmosphereComponent?, GasTileOverlayComponent?>(gridUid, CompOrNull<GridAtmosphereComponent>(gridUid), CompOrNull<GasTileOverlayComponent>(gridUid));

        Entity<MapAtmosphereComponent?>? mapEntity = null;
        var mapUid = _mapManager.GetMapEntityId(mapCoords.MapId);
        if (mapUid != EntityUid.Invalid)
        {
            mapEntity = new Entity<MapAtmosphereComponent?>(mapUid, CompOrNull<MapAtmosphereComponent>(mapUid));
        }

        var centerTile = _mapSystem.CoordinatesToTile(gridUid, grid, coords);

        var visited = new HashSet<Vector2i>();
        var queue = new Queue<(Vector2i Tile, float Distance)>();
        queue.Enqueue((centerTile, 0f));
        visited.Add(centerTile);

        while (queue.Count > 0)
        {
            var (currentTile, currentDistance) = queue.Dequeue();

            if (currentDistance > component.Range)
                continue;

            if (!_mapSystem.TryGetTileRef(gridUid, grid, currentTile, out var tileRef))
                continue;

            var mixture = _atmosphereSystem.GetTileMixture(gridEntity, mapEntity, tileRef.GridIndices, excite: true);
            if (mixture == null)
                continue;

            foreach (var (gas, moles) in component.GasAdjustments)
            {
                mixture.AdjustMoles(gas, moles);
            }

            if (component.Temperature.HasValue)
            {
                mixture.Temperature = Math.Max(component.Temperature.Value, Atmospherics.TCMB);
            }

            var directions = new[] { AtmosDirection.North, AtmosDirection.South, AtmosDirection.East, AtmosDirection.West };
            var offsets = new[] { new Vector2i(0, 1), new Vector2i(0, -1), new Vector2i(1, 0), new Vector2i(-1, 0) };

            for (int i = 0; i < directions.Length; i++)
            {
                var direction = directions[i];
                var offset = offsets[i];
                var neighborTile = currentTile + offset;

                if (visited.Contains(neighborTile))
                    continue;

                if (_atmosphereSystem.IsTileAirBlocked(gridUid, currentTile, direction, grid))
                    continue;

                float neighborDistance = MathF.Sqrt((neighborTile.X - centerTile.X) * (neighborTile.X - centerTile.X) +
                                                    (neighborTile.Y - centerTile.Y) * (neighborTile.Y - centerTile.Y));

                if (neighborDistance > component.Range)
                    continue;

                queue.Enqueue((neighborTile, neighborDistance));
                visited.Add(neighborTile);
            }
        }

        _gasOverlaySystem.UpdateSessions();

        args.Handled = true;
    }
}