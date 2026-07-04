using System.Linq;
using System.Numerics;
using Content.Shared._DV.CosmicCult.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.EntitySystems;

public sealed class CosmicTileDetonationSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var detonateQuery = EntityQueryEnumerator<CosmicTileDetonatorComponent>();
        while (detonateQuery.MoveNext(out var ent, out var comp))
        {
            if (comp.Size.Y > comp.MaxSize.Y || _timing.CurTime < comp.DetonationTimer)
                continue;

            var xform = Transform(ent);
            if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
                continue;
            var gridEnt = ((EntityUid)xform.GridUid, grid);
            if (!_transform.TryGetGridTilePosition(ent, out var tilePos))
                continue;

            var pos = _map.TileCenterToVector(gridEnt, tilePos);
            var bounds = Box2.CenteredAround(pos, comp.Size);
            var boundsMod = Box2.CenteredAround(pos, new Vector2(comp.Size.X - 1, comp.Size.Y - 1));
            var zone = _map.GetLocalTilesIntersecting(ent, grid, bounds).ToList();
            var zoneMod = _map.GetLocalTilesIntersecting(ent, grid, boundsMod).ToList();

            zone = zone.Where(b => !zoneMod.Contains(b)).ToList();
            foreach (var tile in zone)
            {
                Spawn(comp.TileDetonation, _map.GridTileToWorld((EntityUid)xform.GridUid, grid, tile.GridIndices));
            }
            comp.DetonationTimer = comp.DetonateWait + _timing.CurTime;
            comp.Size.X += 2f;
            comp.Size.Y += 2f;
        }
    }
}
