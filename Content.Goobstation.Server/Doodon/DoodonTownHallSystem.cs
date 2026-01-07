using Content.Goobstation.Shared.Doodons;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.Doodons;

/// <summary>
/// Handles Town Hall authority:
/// - Assign buildings to the nearest town hall within radius
/// - Assign doodons to the nearest town hall within radius
/// - Deactivate buildings if no town hall is in range
/// - Maintain TownHall tracking sets (Buildings / Doodons)
/// </summary>
public sealed class DoodonTownHallSystem : EntitySystem
{
    public override void Initialize()
    {
        // Building finished construction / appeared
        SubscribeLocalEvent<DoodonBuildingComponent, MapInitEvent>(OnBuildingMapInit);

        // Doodon spawned / appeared
        SubscribeLocalEvent<DoodonComponent, MapInitEvent>(OnDoodonMapInit);

        // Cleanup on deletion / component removal
        SubscribeLocalEvent<DoodonBuildingComponent, ComponentShutdown>(OnBuildingShutdown);
        SubscribeLocalEvent<DoodonComponent, ComponentShutdown>(OnDoodonShutdown);
    }

    private void OnBuildingMapInit(EntityUid uid, DoodonBuildingComponent component, ref MapInitEvent args)
    {
        AssignBuildingToTownHall(uid, component);
    }

    private void OnDoodonMapInit(EntityUid uid, DoodonComponent component, ref MapInitEvent args)
    {
        AssignDoodonToTownHall(uid, component);
    }

    /// <summary>
    /// Attempts to assign a building to the nearest town hall within influence radius.
    /// If none found, the building is marked inactive.
    /// </summary>
    private void AssignBuildingToTownHall(EntityUid uid, DoodonBuildingComponent building)
    {
        var buildingXform = Transform(uid);

        EntityUid? closestHall = null;
        float closestDistance = float.MaxValue;

        var query = EntityQueryEnumerator<DoodonTownHallComponent>();
        while (query.MoveNext(out var hallUid, out var hall))
        {
            var hallXform = Transform(hallUid);

            // Must be on same map
            if (hallXform.MapID != buildingXform.MapID)
                continue;

            var distance = (hallXform.WorldPosition - buildingXform.WorldPosition).Length();

            // Must be in radius
            if (distance > hall.InfluenceRadius)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestHall = hallUid;
            }
        }

        // Remove from old hall if it had one
        if (building.TownHall is { } oldHallUid && TryComp<DoodonTownHallComponent>(oldHallUid, out var oldHall))
        {
            oldHall.Buildings.Remove(uid);
        }

        // No Town Hall nearby → building is inactive and unassigned
        if (closestHall is null)
        {
            building.TownHall = null;
            building.Active = false;
            return;
        }

        // Assign + activate
        building.TownHall = closestHall;
        building.Active = true;

        // Track on hall
        if (TryComp<DoodonTownHallComponent>(closestHall.Value, out var hallComp))
            hallComp.Buildings.Add(uid);
    }

    /// <summary>
    /// Registers a doodon to the nearest town hall within influence radius.
    /// </summary>
    private void AssignDoodonToTownHall(EntityUid uid, DoodonComponent doodon)
    {
        var doodonXform = Transform(uid);

        EntityUid? closestHall = null;
        float closestDistance = float.MaxValue;

        var query = EntityQueryEnumerator<DoodonTownHallComponent>();
        while (query.MoveNext(out var hallUid, out var hall))
        {
            var hallXform = Transform(hallUid);

            // Must be on same map
            if (hallXform.MapID != doodonXform.MapID)
                continue;

            var distance = (hallXform.WorldPosition - doodonXform.WorldPosition).Length();

            // Must be in radius
            if (distance > hall.InfluenceRadius)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestHall = hallUid;
            }
        }

        // Remove from old hall if it had one
        if (doodon.TownHall is { } oldHallUid && TryComp<DoodonTownHallComponent>(oldHallUid, out var oldHall))
        {
            oldHall.Doodons.Remove(uid);
        }

        if (closestHall is null)
        {
            // No hall in range: leave unassigned (up to you if you want to delete / disable)
            doodon.TownHall = null;
            return;
        }

        doodon.TownHall = closestHall;

        if (TryComp<DoodonTownHallComponent>(closestHall.Value, out var hallComp))
            hallComp.Doodons.Add(uid);
    }

    private void OnBuildingShutdown(EntityUid uid, DoodonBuildingComponent comp, ref ComponentShutdown args)
    {
        if (comp.TownHall is not { } hallUid)
            return;

        if (TryComp<DoodonTownHallComponent>(hallUid, out var hallComp))
            hallComp.Buildings.Remove(uid);
    }

    private void OnDoodonShutdown(EntityUid uid, DoodonComponent comp, ref ComponentShutdown args)
    {
        if (comp.TownHall is not { } hallUid)
            return;

        if (TryComp<DoodonTownHallComponent>(hallUid, out var hallComp))
            hallComp.Doodons.Remove(uid);
    }
}
