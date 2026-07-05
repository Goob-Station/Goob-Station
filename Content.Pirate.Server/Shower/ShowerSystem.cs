using Content.Pirate.Shared.Showers;
using Content.Pirate.Shared.Stains.Components;
using Content.Pirate.Shared.Wetness.Components;
using Content.Pirate.Shared.Wetness.Systems;
using Content.Pirate.Server.Stains;
using Content.Shared.Inventory;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;

namespace Content.Pirate.Server.Shower;

public sealed class ShowerSystem : SharedShowerSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly StainSystem _stains = null!;
    [Dependency] private readonly SharedWetnessSystem _wetness = null!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShowerComponent>();
        while (query.MoveNext(out var uid, out var shower))
        {
            if (!shower.ToggleShower)
                continue;

            shower.StainCleanAccumulator += frameTime;
            if (shower.StainCleanAccumulator < shower.StainCleanInterval)
                continue;

            shower.StainCleanAccumulator = 0f;

            // Only affect what is standing/lying on the shower's own tile. A plain radius bleeds onto
            // neighbouring tiles (e.g. a washing machine next door), wetting things it shouldn't.
            var xform = Transform(uid);
            if (xform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
                continue;

            var tile = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);
            var coords = xform.Coordinates;

            foreach (var (target, _) in _lookup.GetEntitiesInRange<InventoryComponent>(coords, shower.StainCleanRange))
            {
                if (!OnTile(target, gridUid, grid, tile))
                    continue;

                _stains.CleanEntityAndEquipment(target);
                // Full-body wash: wet every worn wettable slot, respecting blockers.
                _wetness.WetEquippedSlots(target, SlotFlags.WITHOUT_POCKET, shower.WetnessPerTick);
            }

            foreach (var (item, _) in _lookup.GetEntitiesInRange<StainableComponent>(coords, shower.StainCleanRange))
            {
                if (!HasComp<InventoryComponent>(item) && OnTile(item, gridUid, grid, tile))
                    _stains.TryCleanStain(item);
            }

            foreach (var (item, wettable) in _lookup.GetEntitiesInRange<WettableComponent>(coords, shower.StainCleanRange))
            {
                // Loose wettable items under the spray get wet too (worn items handled above).
                if (!HasComp<InventoryComponent>(item) && OnTile(item, gridUid, grid, tile))
                    _wetness.AddWetness((item, wettable), shower.WetnessPerTick);
            }
        }
    }

    private bool OnTile(EntityUid target, EntityUid gridUid, MapGridComponent grid, Vector2i tile)
    {
        var xform = Transform(target);
        return xform.GridUid == gridUid && _map.TileIndicesFor(gridUid, grid, xform.Coordinates) == tile;
    }
}
