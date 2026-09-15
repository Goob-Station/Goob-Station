using System.Numerics;
using Content.Server.Respawn;
using Content.Shared.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Gibbing.Systems;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Voidwalker.Kidnapping;

public sealed class VoidwalkerKidnappedSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SpecialRespawnSystem _respawn = default!;
    [Dependency] private readonly StationSystem _station = default!;

    private const int MaxTeleportAttempts = 100;
    private ISawmill _sawmill = null!;

    private Dictionary<EntityUid, int> _teleportFailCount = new();
    private const int MaxTeleportAttemptFails = 10;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("voidwalker-kidnapping");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var kidnappedQuery = EntityQueryEnumerator<VoidwalkerKidnappedComponent>();
        while (kidnappedQuery.MoveNext(out var uid, out var kidnapped))
        {
            if (_timing.CurTime < kidnapped.ExitVoidTime
                || !_mind.TryGetMind(uid, out _, out var mind))
                continue;

            mind.PreventGhosting = false;

            if (TryTeleportToRandomPartOfStation(uid, kidnapped.OriginalMap))
                RemCompDeferred(uid, kidnapped);
        }
    }

    public bool TryTeleportToRandomPartOfStation(EntityUid uid, EntityUid originalMap)
    {
        var xform = Transform(uid);
        if (xform is null)
            return false;

        var destinationMap = Transform(originalMap).MapID;

        if (_station.GetStationInMap(destinationMap) is not { } station)
            return false;

        if (!TryComp<StationDataComponent>(station, out var stationData))
            return false;

        var entityGridUid = _station.GetLargestGrid((station, stationData));

        if (entityGridUid is null
            || xform.MapUid is null)
            return false;

        if (_respawn.TryFindRandomTile(entityGridUid.Value, xform.MapUid.Value, MaxTeleportAttempts, out var randomPos))
            _transform.SetCoordinates(uid, randomPos);
        else
        {
            if (!_teleportFailCount.TryGetValue(uid, out var failCount))
                failCount = 0;

            failCount++;

            _teleportFailCount[uid] = failCount;
            if (failCount >= MaxTeleportAttemptFails)
            {
                _sawmill.Warning($"Could not find station to return {ToPrettyString(uid)} to within {MaxTeleportAttempts * MaxTeleportAttemptFails} attempts. Deleting.");
                Del(uid);
                return false;
            }

            var mapCoordinates = new MapCoordinates(new Vector2(0, 0), xform.MapID);
            _transform.SetMapCoordinates(uid, mapCoordinates);
            _sawmill.Warning($"Could not find station to return {ToPrettyString(uid)} to within {MaxTeleportAttempts}. Returning to default position.");
        }

        _stun.KnockdownOrStun(uid, TimeSpan.FromSeconds(5), true); // whatever, go my magic number
        _popup.PopupEntity(Loc.GetString("voidwalker-kidnap-return"), uid, uid);

        return true;
    }
}
