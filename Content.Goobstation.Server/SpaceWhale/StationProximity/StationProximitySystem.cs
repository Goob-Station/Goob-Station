using Content.Server.Popups;
using Content.Server.Station.Components;
using Content.Goobstation.Server.MobCaller;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.Audio;
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
    [Dependency] private readonly AudioSystem _audio = default!;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(60); // le hardcode major
    private const float MaxDistance = 965; // make it lower for testing idk
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
        var stations = new List<(EntityUid Uid, MapGridComponent Grid, TransformComponent Xform)>();

        while (stationQuery.MoveNext(out var uid, out _, out var grid))
        {
            var xform = Transform(uid);
            stations.Add((uid, grid, xform));
        }

        if (stations.Count == 0)
            return;

        var humanoidQuery = EntityQueryEnumerator<HumanoidAppearanceComponent, MobStateComponent, TransformComponent>();
        while (humanoidQuery.MoveNext(out var uid, out _, out var mobState, out var humanoidXform))
        {
            if (mobState.CurrentState != MobState.Alive)
                continue;

            var sameMap = false;
            foreach (var (_, _, stationXform) in stations)
            {
                if (stationXform.MapUid == humanoidXform.MapUid)
                {
                    sameMap = true;
                    break;
                }
            }
            if (!sameMap)
                continue;

            CheckHumanoidProximity(uid, stations, humanoidXform);
        }
    }

    private void CheckHumanoidProximity(EntityUid humanoid,
        List<(EntityUid Uid, MapGridComponent Grid, TransformComponent Xform)> stations,
        TransformComponent humanoidTransform)
    {
        var isNearStation = false;

        if (humanoidTransform.GridUid != null)
        {
            foreach (var (stationUid, _, _) in stations)
            {
                if (stationUid == humanoidTransform.GridUid)
                {
                    isNearStation = true;
                    break;
                }
            }
        }

        if (!isNearStation) // if not, check the distance #30436
        {
            var humanoidWorldPos = _transform.GetWorldPosition(humanoidTransform);
            var closestDistance = float.MaxValue;

            foreach (var (stationUid, grid, stationXform) in stations)
            {
                if (stationXform.MapUid != humanoidTransform.MapUid)
                    continue;

                var stationWorldPos = _transform.GetWorldPosition(stationXform);
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

    private void HandleFarFromStation(EntityUid entity) // basically handles space whale spawnings
    {
        _popup.PopupEntity(
            Loc.GetString("station-proximity-far-from-station"),
            entity,
            entity,
            PopupType.LargeCaution);

        _audio.PlayEntity(new SoundPathSpecifier("/Audio/_Goobstation/Ambience/SpaceWhale/leviathan-appear.ogg"),
            entity,
            entity,
            AudioParams.Default.WithVolume(1f));

        if (HasComp<MobCallerComponent>(entity))
            return;

        var mobCaller = EnsureComp<MobCallerComponent>(entity);

        mobCaller.SpawnProto = "SpaceLeviathanDespawn";
        mobCaller.MaxAlive = 1; // nuh uh
        mobCaller.MinDistance = 100f; // should be far away
        mobCaller.NeedAnchored = false;
        mobCaller.NeedPower = false;
        mobCaller.SpawnSpacing = TimeSpan.FromSeconds(65); // to give the guy some time to get back to the station + prevent him from like, QSI-ing to the station to summon the worm in the station lmao, also bru these 5 seconds are really important
        mobCaller.SpawnedEntities = new List<EntityUid>(); // crashes without this
    }
}
