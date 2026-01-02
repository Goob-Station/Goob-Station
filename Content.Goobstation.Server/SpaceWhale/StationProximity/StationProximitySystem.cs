using Content.Goobstation.Common.CCVar;
using Content.Server.Popups;
using Content.Server.Station.Components;
using Content.Goobstation.Server.MobCaller;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
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
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(60); // le hardcode major
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
        if (!_cfg.GetCVar(GoobCVars.SpaceWhaleSpawn))
            return;

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

            // grab the  max distance from cvar
            isNearStation = closestDistance <= _cfg.GetCVar(GoobCVars.SpaceWhaleSpawnDistance);
        }

        if (isNearStation)
        {
            // if near station, remove the tracking component and delete the dummy entity
            if (TryComp<SpaceWhaleTargetComponent>(humanoid, out var whaleTarget))
            {
                QueueDel(whaleTarget.Entity);
                RemComp<SpaceWhaleTargetComponent>(humanoid);
            }
        }
        else
            HandleFarFromStation(humanoid);
    }

    private void HandleFarFromStation(EntityUid entity) // basically handles space whale spawnings
    {
        if (HasComp<SpaceWhaleTargetComponent>(entity))
            return;

        _popup.PopupEntity(
            Loc.GetString("station-proximity-far-from-station"),
            entity,
            entity,
            PopupType.LargeCaution);

        _audio.PlayEntity(new SoundPathSpecifier("/Audio/_Goobstation/Ambience/SpaceWhale/leviathan-appear.ogg"),
            entity,
            entity,
            AudioParams.Default.WithVolume(1f));

        // Spawn a dummy entity at the player's location and lock it onto the player
        var dummy = Spawn(null, Transform(entity).Coordinates);
        _transform.SetParent(dummy, entity);
        var mobCaller = EnsureComp<MobCallerComponent>(dummy); // assign the goidacaller to the dummy

        mobCaller.SpawnProto = "SpaceLeviathanDespawn";
        mobCaller.MaxAlive = 1; // nuh uh
        mobCaller.MinDistance = 100f; // should be far away
        mobCaller.NeedAnchored = false;
        mobCaller.NeedPower = false;
        mobCaller.SpawnSpacing = TimeSpan.FromSeconds(65); // to give the guy some time to get back to the station + prevent him from like, QSI-ing to the station to summon the worm in the station lmao, also bru these 5 seconds are really important

        var targetComp = EnsureComp<SpaceWhaleTargetComponent>(entity);// track the dummy on the player
        targetComp.Entity = dummy;
    }
}
