using Content.Goobstation.Shared.Voodoo;
using Content.Server.Construction;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.Doodons;

/// <summary>
/// Handles Town Hall authority, building assignment,
/// radius checks, and population rules.
/// </summary>
public sealed class DoodonTownHallSystem : EntitySystem
{
    public override void Initialize()
    {
        // Building finished construction
        SubscribeLocalEvent<DoodonBuildingComponent, MapInitEvent>(OnBuildingBuilt);

        // Doodon spawned
        SubscribeLocalEvent<DoodonComponent, MapInitEvent>(OnDoodonSpawned);

        // Entity cleanup
        SubscribeLocalEvent<EntityTerminatingEvent>(OnEntityTerminating);
    }


    /// <summary>
    /// Called when a doodon building finishes construction.
    /// Finds the nearest Town Hall and assigns ownership.
    /// </summary>
    private void OnBuildingBuilt(EntityUid uid, DoodonBuildingComponent component, ref MapInitEvent args)
    {
        AssignBuildingToTownHall(uid, component);
    }

    /// <summary>
    /// Called when a doodon spawns.
    /// Registers it to the nearest Town Hall.
    /// </summary>
    private void OnDoodonSpawned(EntityUid uid, DoodonComponent component, ref MapInitEvent args)
    {
        AssignDoodonToTownHall(uid, component);
    }

    /// <summary>
    /// Attempts to assign a building to the nearest Town Hall.
    /// Disables the building if none are in range.
    /// </summary>
    private void AssignBuildingToTownHall(EntityUid uid, DoodonBuildingComponent building)
    {
        var buildingXform = Transform(uid);

        EntityUid? closestHall = null;
        float closestDistance = float.MaxValue;

        // Enumerate all Town Halls with their EntityUid
        var query = EntityQueryEnumerator<DoodonTownHallComponent>();

        while (query.MoveNext(out var hallUid, out var hall))
        {
            var hallXform = Transform(hallUid);

            // Distance between building and town hall
            var distance =
                (hallXform.WorldPosition - buildingXform.WorldPosition).Length();

            // Only accept town halls within influence radius
            if (distance <= hall.InfluenceRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestHall = hallUid;
            }
        }

        // No Town Hall nearby → building is inactive
        if (closestHall == null)
        {
            building.TownHall = null;
            building.Active = false;
            return;
        }

        // Register building to Town Hall
        building.TownHall = closestHall;
        building.Active = true;

        Comp<DoodonTownHallComponent>(closestHall.Value)
            .Buildings.Add(uid);
    }

    /// <summary>
    /// Registers a doodon to the nearest Town Hall.
    /// </summary>
    private void AssignDoodonToTownHall(EntityUid uid, DoodonComponent doodon)
    {
        var doodonXform = Transform(uid);

        EntityUid? closestHall = null;
        float closestDistance = float.MaxValue;

        var query = EntityQueryEnumerator<DoodonTownHallComponent>();

        while (query.MoveNext(out var hallUid, out var hall))
        {
            var distance =
                (Transform(hallUid).WorldPosition - doodonXform.WorldPosition).Length();

            // Only accept town halls within influence radius
            if (distance <= hall.InfluenceRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestHall = hallUid;
            }

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestHall = hallUid;
            }
        }

        if (closestHall == null)
            return;

        doodon.TownHall = closestHall;
        Comp<DoodonTownHallComponent>(closestHall.Value)
            .Doodons.Add(uid);
    }

    /// <summary>
    /// Removes entities from Town Hall tracking when deleted.
    /// </summary>
    private void OnEntityTerminating(ref EntityTerminatingEvent ev)
    {
        if (TryComp<DoodonBuildingComponent>(ev.Entity, out var building) &&
            building.TownHall is { } hall &&
            TryComp<DoodonTownHallComponent>(hall, out var hallComp))
        {
            hallComp.Buildings.Remove(ev.Entity);
        }

        if (TryComp<DoodonComponent>(ev.Entity, out var doodon) &&
            doodon.TownHall is { } hall2 &&
            TryComp<DoodonTownHallComponent>(hall2, out var hallComp2))
        {
            hallComp2.Doodons.Remove(ev.Entity);
        }
    }
}
