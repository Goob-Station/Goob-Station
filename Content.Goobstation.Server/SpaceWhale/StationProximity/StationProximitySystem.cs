using Content.Server.Popups;
using Content.Server.Station.Components;
using Content.Goobstation.Server.MobCaller;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.SpaceWhale.StationProximity;

// used by space whales so think twice beofre using it for yourself somewhere else
// also half of this was taken from wizden #30436 and redone for whale purposes
public sealed class StationProximitySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(60); // le hardcode major
    private const float MaxDistance = 200; // todo 1000 or so
    private TimeSpan _nextCheck = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();
        _nextCheck = _timing.CurTime + CheckInterval;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextCheck)
            return;

        _nextCheck = _timing.CurTime + CheckInterval;
        CheckStationProximity();
    }

    private void CheckStationProximity()
    {
        var stationQuery = EntityQueryEnumerator<BecomesStationComponent, MapGridComponent>();
        var stations = new List<(EntityUid Uid, MapGridComponent Grid)>();

        while (stationQuery.MoveNext(out var uid, out _, out var grid))
        {
            stations.Add((uid, grid));
        }

        if (stations.Count == 0)
            return;

        var humanoidQuery = EntityQueryEnumerator<HumanoidAppearanceComponent, MobStateComponent>();
        while (humanoidQuery.MoveNext(out var uid, out _, out var mobState))
        {
            if (mobState.CurrentState != MobState.Alive)
                continue;

            CheckHumanoidProximity(uid, stations);
        }
    }

    private void CheckHumanoidProximity(EntityUid humanoid, List<(EntityUid Uid, MapGridComponent Grid)> stations)
    {
        var humanoidTransform = Transform(humanoid);
        var isNearStation = false;

        if (humanoidTransform.GridUid != null)
        {
            foreach (var (stationUid, _) in stations)
            { // check if the guy is on the station grid
                if (stationUid == humanoidTransform.GridUid)
                {
                    isNearStation = true;
                    break;
                }
            }
        }

        if (!isNearStation) // if not, check the distance #30436
        {
            var humanoidWorldPos = _transform.GetWorldPosition(humanoid);
            var closestDistance = float.MaxValue;

            foreach (var (stationUid, grid) in stations)
            {
                var stationWorldPos = _transform.GetWorldPosition(stationUid);
                var distance = (humanoidWorldPos - stationWorldPos).Length();

                if (grid.LocalAABB.Size.Length() > 0)
                {
                    var gridRadius = grid.LocalAABB.Size.Length() / 2f; // it needs to be halved to get correct mesurements
                    distance = Math.Max(0, distance - gridRadius);
                }

                closestDistance = Math.Min(closestDistance, distance);
            }

            isNearStation = closestDistance <= MaxDistance;
        }
        HandleStationProximity(humanoid, isNearStation);
    }

    /// <summary>
    /// Proccess the entity near the station for space whales spawning
    /// </summary>
    private void HandleStationProximity(EntityUid entity, bool isNearStation)
    {
        // ok this is goida but it saves like 300 lines because the logic is already here
        var hasMobCaller = HasComp<MobCallerComponent>(entity);

        if (isNearStation)
        {
            if (hasMobCaller)// if the entity is near the station and has mobcallercomp, delete it, we dont want whales to spawn near the station do we
                RemComp<MobCallerComponent>(entity);
        }
        else
        {
            if (!hasMobCaller) // if the entity is far from the station
                HandleFarFromStation(entity);
        }
    }

    private void HandleFarFromStation(EntityUid entity)
    {
        _popup.PopupEntity(
            Loc.GetString("station-proximity-far-from-station"),
            entity,
            entity,
            PopupType.LargeCaution);

        if (HasComp<MobCallerComponent>(entity))
            return;

        var mobCaller = AddComp<MobCallerComponent>(entity);

        mobCaller.SpawnProto = "SpaceWhaleTest"; // todo
        mobCaller.MaxAlive = 1; // nuh uh
        mobCaller.MinDistance = 100f; // should be far away
        mobCaller.NeedAnchored = false;
        mobCaller.NeedPower = false;
        mobCaller.SpawnedEntities = new List<EntityUid>(); // crashes without this
    }
}
