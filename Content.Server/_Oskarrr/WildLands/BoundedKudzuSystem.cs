// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Spreader;
using Content.Shared._Oskarrr.WildLands;
using Content.Shared.Spreader;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Oskarrr.WildLands;

/// <summary>
/// Limits kudzu spread to a small radius around a mother tile and re-enables spreading
/// while empty tiles remain inside that radius.
/// </summary>
public sealed class BoundedKudzuSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SpreaderSystem _spreader = default!;

    private float _recheckAccum;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BoundedKudzuComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BoundedKudzuComponent, SpreadNeighborsEvent>(OnSpread, before: [typeof(KudzuSystem)]);
    }

    private void OnMapInit(Entity<BoundedKudzuComponent> ent, ref MapInitEvent args)
    {
        EnsureOrigin(ent);

        // Mother seed: random patch radius 2–3 tiles.
        if (ent.Comp.SpreadAs != null)
            ent.Comp.MaxRadius = _random.Next(2, 4);
    }

    private void EnsureOrigin(Entity<BoundedKudzuComponent> ent)
    {
        if (ent.Comp.OriginIndices != null && ent.Comp.OriginGrid != null)
            return;

        var xform = Transform(ent);
        if (xform.GridUid is not { } gridUid || !TryComp(gridUid, out MapGridComponent? grid))
            return;

        ent.Comp.OriginGrid = gridUid;
        ent.Comp.OriginIndices = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);
    }

    private void OnSpread(Entity<BoundedKudzuComponent> ent, ref SpreadNeighborsEvent args)
    {
        EnsureOrigin(ent);

        if (ent.Comp.OriginIndices is not { } origin || ent.Comp.OriginGrid is not { } originGrid)
        {
            args.NeighborFreeTiles.Clear();
            return;
        }

        // Drop tiles outside the mother radius (and off the mother grid).
        for (var i = args.NeighborFreeTiles.Count - 1; i >= 0; i--)
        {
            var (grid, tile) = args.NeighborFreeTiles[i];
            if (tile.GridUid != originGrid
                || Chebyshev(tile.GridIndices, origin) > ent.Comp.MaxRadius)
            {
                args.NeighborFreeTiles.RemoveSwap(i);
            }
        }

        if (!TryComp<KudzuComponent>(ent, out var kudzu) || kudzu.GrowthLevel < 3)
            return;

        if (args.NeighborFreeTiles.Count == 0)
            return;

        kudzu.TimeAccumulated += SpreaderSystem.SpreadCooldownSeconds;
        if (kudzu.TimeAccumulated < 1f)
        {
            // Prevent stock KudzuSystem from also spreading this tick.
            // Keep ActiveEdgeSpreader — KudzuSystem would RemComp on empty neighbors.
            args.NeighborFreeTiles.Clear();
            EnsureComp<ActiveEdgeSpreaderComponent>(ent);
            return;
        }

        kudzu.TimeAccumulated = 0f;

        if (!_random.Prob(kudzu.SpreadChance))
        {
            args.NeighborFreeTiles.Clear();
            EnsureComp<ActiveEdgeSpreaderComponent>(ent);
            return;
        }

        var proto = ent.Comp.SpreadAs?.Id
                    ?? MetaData(ent).EntityPrototype?.ID;

        if (proto == null)
        {
            args.NeighborFreeTiles.Clear();
            return;
        }

        foreach (var neighbor in args.NeighborFreeTiles)
        {
            if (args.Updates <= 0)
                break;

            var spawned = Spawn(proto,
                _map.GridTileToLocal(neighbor.Tile.GridUid, neighbor.Grid, neighbor.Tile.GridIndices));

            if (TryComp<BoundedKudzuComponent>(spawned, out var childBound))
            {
                childBound.OriginIndices = origin;
                childBound.OriginGrid = originGrid;
                childBound.MaxRadius = ent.Comp.MaxRadius;
                childBound.SpreadAs = null; // children spread as themselves
            }

            args.Updates--;
        }

        // Stop stock KudzuSystem from double-spawning mother prototypes.
        args.NeighborFreeTiles.Clear();

        if (HasExpandableNeighbor(ent, Transform(ent), originGrid,
                Comp<MapGridComponent>(originGrid), origin, ent.Comp.MaxRadius))
            EnsureComp<ActiveEdgeSpreaderComponent>(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _recheckAccum += frameTime;
        if (_recheckAccum < 2f)
            return;
        _recheckAccum = 0f;

        // KudzuSystem removes ActiveEdgeSpreader when no free neighbors remain;
        // re-enable while empty tiles still exist inside the mother radius.
        var query = EntityQueryEnumerator<BoundedKudzuComponent, KudzuComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var bound, out var kudzu, out var xform))
        {
            if (kudzu.GrowthLevel < 3)
                continue;

            if (HasComp<ActiveEdgeSpreaderComponent>(uid))
                continue;

            if (bound.OriginIndices is not { } origin || bound.OriginGrid is not { } originGrid)
                continue;

            if (xform.GridUid != originGrid || !TryComp(originGrid, out MapGridComponent? grid))
                continue;

            if (!HasExpandableNeighbor(uid, xform, originGrid, grid, origin, bound.MaxRadius))
                continue;

            EnsureComp<ActiveEdgeSpreaderComponent>(uid);
        }
    }

    private bool HasExpandableNeighbor(
        EntityUid uid,
        TransformComponent xform,
        EntityUid originGrid,
        MapGridComponent grid,
        Vector2i origin,
        int maxRadius)
    {
        _spreader.GetNeighbors(uid, xform, "Kudzu", out var freeTiles, out _, out _);
        foreach (var (_, tile) in freeTiles)
        {
            if (tile.GridUid == originGrid && Chebyshev(tile.GridIndices, origin) <= maxRadius)
                return true;
        }

        return false;
    }

    private static int Chebyshev(Vector2i a, Vector2i b)
        => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
}
