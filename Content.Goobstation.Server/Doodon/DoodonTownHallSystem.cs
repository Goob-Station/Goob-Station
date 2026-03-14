using Content.Goobstation.Shared.Doodons;
using Robust.Shared.GameObjects;
using Content.Shared.Examine;
using Robust.Shared.Utility;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;

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
    private const float RefreshInterval = 2.0f; // seconds (tune as you like)
    private float _refreshAccum;
    public override void Initialize()
    {
        base.Initialize();
        // Building finished construction / appeared
        SubscribeLocalEvent<DoodonBuildingComponent, MapInitEvent>(OnBuildingMapInit);

        // Doodon spawned / appeared
        SubscribeLocalEvent<DoodonComponent, MapInitEvent>(OnDoodonMapInit);

        // Cleanup on deletion / component removal
        SubscribeLocalEvent<DoodonBuildingComponent, ComponentShutdown>(OnBuildingShutdown);
        SubscribeLocalEvent<DoodonComponent, ComponentShutdown>(OnDoodonShutdown);
        SubscribeLocalEvent<DoodonTownHallComponent, ExaminedEvent>(OnHallExamined);
        SubscribeLocalEvent<DoodonTownHallComponent, ToggleTownHallRadiusEvent>(OnToggleRadius);
        SubscribeLocalEvent<DoodonTownHallComponent, ComponentShutdown>(OnHallShutdown);
        SubscribeLocalEvent<DoodonComponent, MobStateChangedEvent>(OnDoodonMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _refreshAccum += frameTime;
        if (_refreshAccum < RefreshInterval)
            return;

        _refreshAccum = 0f;

        // If there are no halls, nothing to connect to.
        if (!AnyTownHalls())
            return;

        RefreshBuildings();
        RefreshDoodons();
    }

    private bool AnyTownHalls()
    {
        var halls = EntityQueryEnumerator<DoodonTownHallComponent>();
        return halls.MoveNext(out _, out _);
    }

    private void RefreshBuildings()
    {
        var buildings = EntityQueryEnumerator<DoodonBuildingComponent>();
        while (buildings.MoveNext(out var uid, out var building))
        {
            if (Deleted(uid))
                continue;

            AssignBuildingToTownHall(uid, building);
        }
    }

    private void RefreshDoodons()
    {
        var doodons = EntityQueryEnumerator<DoodonComponent>();
        while (doodons.MoveNext(out var uid, out var doodon))
        {
            if (Deleted(uid))
                continue;

            AssignDoodonToTownHall(uid, doodon);
        }
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

        // If we already have a hall and it still exists, KEEP IT.
        // This is what stops population from dropping when leaving influence.
        if (doodon.TownHall is { } existingHall && !Deleted(existingHall) && HasComp<DoodonTownHallComponent>(existingHall))
            return;

        EntityUid? closestHall = null;
        float closestDistance = float.MaxValue;

        var query = EntityQueryEnumerator<DoodonTownHallComponent>();
        while (query.MoveNext(out var hallUid, out var hall))
        {
            var hallXform = Transform(hallUid);

            if (hallXform.MapID != doodonXform.MapID)
                continue;

            var distance = (hallXform.WorldPosition - doodonXform.WorldPosition).Length();

            // Only assign if they are currently inside some hall's influence radius.
            if (distance > hall.InfluenceRadius)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestHall = hallUid;
            }
        }

        // No hall in range AND we had no valid hall -> remain unassigned.
        if (closestHall is null)
        {
            doodon.TownHall = null;
            return;
        }

        // Assign + track on hall
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

    private void OnHallExamined(EntityUid uid, DoodonTownHallComponent comp, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        // Connected buildings (active + assigned to this hall)
        var connectedBuildings = 0;
        foreach (var b in comp.Buildings)
        {
            if (Deleted(b) || !TryComp<DoodonBuildingComponent>(b, out var building))
                continue;

            if (!building.Active)
                continue;

            if (building.TownHall != uid)
                continue;

            connectedBuildings++;
        }

        // Housing stats + total pop (CountsTowardPopulation respected inside GetHousingStats)
        GetHousingStats(uid, comp,
            out var workerCap, out var warriorCap, out var moodonCap,
            out var workerPop, out var warriorPop, out var moodonPop,
            out var totalPopulation);

        // Build ONE block so ordering stays stable and no weird gaps happen.
        var text =
            $"{Loc.GetString("doodon-townhall-population", ("current", totalPopulation))}\n" +
            $"{Loc.GetString("doodon-townhall-buildings", ("count", connectedBuildings))}\n" +
            $"{Loc.GetString("doodon-townhall-workers", ("count", ColorizeCount(workerPop, workerCap)))}\n" +
            $"{Loc.GetString("doodon-townhall-warriors", ("count", ColorizeCount(warriorPop, warriorCap)))}\n" +
            $"{Loc.GetString("doodon-townhall-moodons", ("count", ColorizeCount(moodonPop, moodonCap)))}";

        args.PushMarkup(text);
    }


    private void GetHousingStats(
    EntityUid hallUid,
    DoodonTownHallComponent hall,
    out int workerCap, out int warriorCap, out int moodonCap,
    out int workerPop, out int warriorPop, out int moodonPop,
    out int totalPopulation)
    {
        workerCap = warriorCap = moodonCap = 0;
        workerPop = warriorPop = moodonPop = 0;
        totalPopulation = 0;

        // Capacity from connected buildings
        foreach (var b in hall.Buildings)
        {
            if (Deleted(b) || !TryComp<DoodonBuildingComponent>(b, out var building))
                continue;

            if (!building.Active || building.TownHall != hallUid)
                continue;

            switch (building.HousingType)
            {
                case DoodonHousingType.Worker:
                    workerCap += building.HousingCapacity;
                    break;
                case DoodonHousingType.Warrior:
                    warriorCap += building.HousingCapacity;
                    break;
                case DoodonHousingType.Moodon:
                    moodonCap += building.HousingCapacity;
                    break;
            }
        }

        // Occupancy from doodons
        foreach (var d in hall.Doodons)
        {
            if (Deleted(d) || !TryComp<DoodonComponent>(d, out var doodon))
                continue;

            // Ignore dead doodons so population doesn't count corpses
            if (TryComp<MobStateComponent>(d, out var mob) && mob.CurrentState == MobState.Dead)
                continue;

            switch (doodon.RequiredHousing)
            {
                case DoodonHousingType.Worker: workerPop++; break;
                case DoodonHousingType.Warrior: warriorPop++; break;
                case DoodonHousingType.Moodon: moodonPop++; break;
            }

            if (doodon.CountsTowardPopulation)
                totalPopulation++;
        }

    }

    private void OnToggleRadius(EntityUid uid, DoodonTownHallComponent comp, ToggleTownHallRadiusEvent args)
    {
        comp.ShowInfluence = !comp.ShowInfluence;
        Dirty(uid, comp);
    }

    private static string ColorizeCount(int population, int capacity)
    {
        var color =
            population > capacity ? "red" :
            population < capacity ? "yellow" :
            "green";

        return $"[color={color}]{population}/{capacity}[/color]";
    }

    private void OnHallShutdown(EntityUid uid, DoodonTownHallComponent comp, ref ComponentShutdown args)
    {
        // Deactivate + unassign buildings that pointed at this hall.
        foreach (var b in comp.Buildings)
        {
            if (Deleted(b) || !TryComp<DoodonBuildingComponent>(b, out var building))
                continue;

            if (building.TownHall != uid)
                continue;

            building.TownHall = null;
            building.Active = false;

            // IMPORTANT:
            // Don't Dirty() unless DoodonBuildingComponent is networked.
            // If you need the client UI to update, trigger it via Appearance / Examine, not Dirty on non-networked comps.
        }

        // Unassign doodons too (optional but usually correct)
        foreach (var d in comp.Doodons)
        {
            if (Deleted(d) || !TryComp<DoodonComponent>(d, out var doodon))
                continue;

            if (doodon.TownHall != uid)
                continue;

            doodon.TownHall = null;
        }

        // Clear sets (not required but keeps refs clean)
        comp.Buildings.Clear();
        comp.Doodons.Clear();
    }
    private void OnDoodonMobStateChanged(EntityUid uid, DoodonComponent comp, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (comp.TownHall is not { } hallUid)
            return;

        if (TryComp<DoodonTownHallComponent>(hallUid, out var hallComp))
            hallComp.Doodons.Remove(uid);
    }
}
