// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Maps;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Random;

namespace Content.Shared._Oskarrr.WildLandsFauna;

public sealed class GrassGrazerSystem : EntitySystem
{
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<GrassGrazerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var grazer, out var xform))
        {
            grazer.Accumulator += frameTime;
            if (grazer.Accumulator < grazer.EatInterval)
                continue;

            grazer.Accumulator = 0f;

            // Stagger so packs don't all eat the same tick.
            if (!_random.Prob(0.65f))
                continue;

            if (!_turf.TryGetTileRef(xform.Coordinates, out var tileRef))
                continue;

            var tileId = _tileDef[tileRef.Value.Tile.TypeId].ID;
            if (!grazer.EdibleTiles.Contains(tileId))
                continue;

            if (xform.GridUid is not { } gridUid || !TryComp(gridUid, out MapGridComponent? grid))
                continue;

            if (!_tileDef.TryGetDefinition(grazer.ReplaceTile, out var replacement))
                continue;

            _map.SetTile(gridUid, grid, tileRef.Value.GridIndices, new Tile(replacement.TileId));

            if (HasComp<HungerComponent>(uid))
                _hunger.ModifyHunger(uid, grazer.HungerRestore);
        }
    }
}
