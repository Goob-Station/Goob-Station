using Content.Goobstation.Maths.FixedPoint;
using Content.Pirate.Shared.Showers;
using Content.Pirate.Shared.Wetness.Components;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;

namespace Content.Pirate.Server.Shower;

public sealed class ShowerSystem : SharedShowerSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = null!;
    [Dependency] private readonly ReactiveSystem _reactive = null!;
    [Dependency] private readonly PuddleSystem _puddle = null!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShowerComponent>();
        while (query.MoveNext(out var uid, out var shower))
        {
            shower.SprayAccumulator += frameTime;
            if (shower.SprayAccumulator < shower.SprayInterval)
                continue;

            shower.SprayAccumulator = 0f;

            if (!_solution.TryGetSolution(uid, shower.SolutionName, out var tank, out var solution))
                continue;

            var regen = shower.ToggleShower ? shower.RegenOn : shower.RegenOff;
            var room = solution.AvailableVolume;
            if (regen > FixedPoint2.Zero && room > FixedPoint2.Zero)
                _solution.TryAddReagent(tank.Value, shower.Reagent, FixedPoint2.Min(regen, room));

            if (!shower.ToggleShower)
                continue;

            if (solution.Volume < shower.SprayAmount)
            {
                SetShower(uid, false, shower);
                continue;
            }

            var spray = _solution.SplitSolution(tank.Value, shower.SprayAmount);
            Spray(uid, shower, spray);
        }
    }

    private void Spray(EntityUid uid, ShowerComponent shower, Solution spray)
    {
        var xform = Transform(uid);
        if (xform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var tile = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);
        var coords = xform.Coordinates;

        var wetAnything = false;

        // Mobs react through worn clothing and bare-body stains.
        foreach (var (target, _) in _lookup.GetEntitiesInRange<InventoryComponent>(coords, shower.SprayRange))
        {
            if (!OnTile(target, gridUid, grid, tile))
                continue;

            // Each target gets an independent dose.
            _reactive.DoEntityReaction(target, spray.Clone(), ReactionMethod.Touch);
            wetAnything = true;
        }

        // Loose wettable items under the spray.
        foreach (var (item, _) in _lookup.GetEntitiesInRange<WettableComponent>(coords, shower.SprayRange))
        {
            if (HasComp<InventoryComponent>(item) || !OnTile(item, gridUid, grid, tile))
                continue;

            _reactive.DoEntityReaction(item, spray.Clone(), ReactionMethod.Touch);
            wetAnything = true;
        }

        if (!wetAnything)
            _puddle.TrySpillAt(coords, spray, out _, sound: false);
    }

    private bool OnTile(EntityUid target, EntityUid gridUid, MapGridComponent grid, Vector2i tile)
    {
        var xform = Transform(target);
        return xform.GridUid == gridUid && _map.TileIndicesFor(gridUid, grid, xform.Coordinates) == tile;
    }
}
