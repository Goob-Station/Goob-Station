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

            // Passive refill: faster while off, slower while running.
            var regen = shower.ToggleShower ? shower.RegenOn : shower.RegenOff;
            var room = solution.AvailableVolume;
            if (regen > FixedPoint2.Zero && room > FixedPoint2.Zero)
                _solution.TryAddReagent(tank.Value, shower.Reagent, FixedPoint2.Min(regen, room));

            if (!shower.ToggleShower)
                continue;

            // Not enough water for a full dose: shut off so the faster off-regen can refill it.
            if (solution.Volume < shower.SprayAmount)
            {
                SetShower(uid, false, shower);
                continue;
            }

            var spray = _solution.SplitSolution(tank.Value, shower.SprayAmount);
            Spray(uid, shower, spray);
        }
    }

    /// <summary>
    /// Applies one spray dose to the shower's own tile. Mobs and loose wettable items get the water
    /// reaction (wetting + gradual stain dilution via the wetness system); if nothing wettable is
    /// there, the dose hits the floor as a puddle.
    /// </summary>
    private void Spray(EntityUid uid, ShowerComponent shower, Solution spray)
    {
        var xform = Transform(uid);
        if (xform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var tile = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);
        var coords = xform.Coordinates;

        var wetAnything = false;

        // Mobs: their worn clothing gets wet and stains diluted via the water reaction.
        foreach (var (target, _) in _lookup.GetEntitiesInRange<InventoryComponent>(coords, shower.SprayRange))
        {
            if (!OnTile(target, gridUid, grid, tile))
                continue;

            _reactive.DoEntityReaction(target, spray, ReactionMethod.Touch);
            wetAnything = true;
        }

        // Loose wettable items under the spray (worn items are handled above via the mob).
        foreach (var (item, _) in _lookup.GetEntitiesInRange<WettableComponent>(coords, shower.SprayRange))
        {
            if (HasComp<InventoryComponent>(item) || !OnTile(item, gridUid, grid, tile))
                continue;

            _reactive.DoEntityReaction(item, spray, ReactionMethod.Touch);
            wetAnything = true;
        }

        // Nothing to wet: the water pools on the floor.
        if (!wetAnything)
            _puddle.TrySpillAt(coords, spray, out _, sound: false);
    }

    private bool OnTile(EntityUid target, EntityUid gridUid, MapGridComponent grid, Vector2i tile)
    {
        var xform = Transform(target);
        return xform.GridUid == gridUid && _map.TileIndicesFor(gridUid, grid, xform.Coordinates) == tile;
    }
}
