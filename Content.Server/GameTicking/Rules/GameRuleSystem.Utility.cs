using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking.Components;
using Content.Shared.Random.Helpers;
using Robust.Shared.Collections;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.GameTicking.Rules;

public abstract partial class GameRuleSystem<T> where T: IComponent
{
    [Dependency] private readonly StationSystem _station = default!; // Goobstation

    protected EntityQueryEnumerator<ActiveGameRuleComponent, T, GameRuleComponent> QueryActiveRules()
    {
        return EntityQueryEnumerator<ActiveGameRuleComponent, T, GameRuleComponent>();
    }

    /// <summary>
    /// Queries all gamerules, regardless of if they're active or not.
    /// </summary>
    protected EntityQueryEnumerator<T, GameRuleComponent> QueryAllRules()
    {
        return EntityQueryEnumerator<T, GameRuleComponent>();
    }

    /// <summary>
    ///     Utility function for finding a random event-eligible station entity
    /// </summary>
    protected bool TryGetRandomStation([NotNullWhen(true)] out EntityUid? station, Func<EntityUid, bool>? filter = null)
    {
        var stations = new ValueList<EntityUid>(Count<StationEventEligibleComponent>());

        filter ??= _ => true;
        var query = AllEntityQuery<StationEventEligibleComponent>();

        while (query.MoveNext(out var uid, out _))
        {
            if (!filter(uid))
                continue;

            stations.Add(uid);
        }

        if (stations.Count == 0)
        {
            station = null;
            return false;
        }

        // TODO: Engine PR.
        station = stations[RobustRandom.Next(stations.Count)];
        return true;
    }

    protected bool TryFindRandomTile(out Vector2i tile,
        [NotNullWhen(true)] out EntityUid? targetStation,
        out EntityUid targetGrid,
        out EntityCoordinates targetCoords)
    {
        tile = default;
        targetStation = EntityUid.Invalid;
        targetGrid = EntityUid.Invalid;
        targetCoords = EntityCoordinates.Invalid;
        if (TryGetRandomStation(out targetStation))
        {
            return TryFindRandomTileOnStation((targetStation.Value, Comp<StationDataComponent>(targetStation.Value)),
                out tile,
                out targetGrid,
                out targetCoords);
        }

        return false;
    }

    // Goobstation start
    // Goobstation - refactored this method. Split into 3 smaller methods and made it so that it picks main station grid.
    protected bool TryFindRandomTileOnStation(Entity<StationDataComponent> station,
        out Vector2i tile,
        out EntityUid targetGrid,
        out EntityCoordinates targetCoords)
    {
        tile = default;
        targetCoords = EntityCoordinates.Invalid;
        targetGrid = EntityUid.Invalid;

        if (GetStationMainGrid(station.Comp) is not { } grid)
            return false;

        targetGrid = grid.Owner;
        return TryFindTileOnGrid(grid, out tile, out targetCoords);
    }

    protected Entity<MapGridComponent>? GetStationMainGrid(StationDataComponent station)
    {
        if ((station.Grids.FirstOrNull(HasComp<BecomesStationComponent>) ?? _station.GetLargestGrid(station)) is not
            { } grid || !TryComp(grid, out MapGridComponent? gridComp))
            return null;

        return (grid, gridComp);
    }

    protected bool TryFindTileOnGrid(Entity<MapGridComponent> grid,
        out Vector2i tile,
        out EntityCoordinates targetCoords,
        int tries = 10)
    {
        tile = default;
        targetCoords = EntityCoordinates.Invalid;

        var aabb = grid.Comp.LocalAABB;

        for (var i = 0; i < tries; i++)
        {
            var randomX = RobustRandom.Next((int) aabb.Left, (int) aabb.Right);
            var randomY = RobustRandom.Next((int) aabb.Bottom, (int) aabb.Top);

            tile = new Vector2i(randomX, randomY);
            if (_atmosphere.IsTileSpace(grid.Owner, Transform(grid.Owner).MapUid, tile)
                || _atmosphere.IsTileAirBlocked(grid.Owner, tile, mapGridComp: grid.Comp))
                continue;

            targetCoords = _map.GridTileToLocal(grid.Owner, grid.Comp, tile);
            return true;
        }

        return false;
    }
    // Goobstation end

    protected void ForceEndSelf(EntityUid uid, GameRuleComponent? component = null)
    {
        GameTicker.EndGameRule(uid, component);
    }
}
