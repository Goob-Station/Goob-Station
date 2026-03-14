using System.Numerics;
using Content.Goobstation.Common.Footprints;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Throwing;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Footprints;

public sealed class FootprintSpillOnLandSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly FootprintSystem _footprint = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FootprintSpillOnLandComponent, LandEvent>(OnLand);
    }

    private void OnLand(Entity<FootprintSpillOnLandComponent> ent, ref LandEvent args)
    {
        if (!_solution.TryGetSolution(ent.Owner, FootprintSystem.FootprintOwnerSolution, out var solution, out var sol))
            return;

        var xform = Transform(ent);
        var (pos, rot) = _transform.GetWorldPositionRotation(xform);
        var coords = new MapCoordinates(pos, xform.MapID);

        if (!_mapMan.TryFindGridAt(coords, out var gridUid, out var grid))
        {
            if (ent.Comp.DeleteEntity)
                QueueDel(ent);
            return;
        }

        var tile = _map.TileIndicesFor((gridUid, grid), coords);

        var state = _random.Pick(ent.Comp.States);
        var sprite = new SpriteSpecifier.Rsi(ent.Comp.Sprite, state);
        var volume = sol.Volume;

        _footprint.UpdateFootprint(solution.Value,
            (gridUid, grid),
            tile,
            xform.Coordinates,
            rot,
            volume,
            ent.Comp.Alpha,
            sprite);

        if (ent.Comp.DeleteEntity)
            QueueDel(ent);
    }
}
