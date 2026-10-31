using System.Linq;
using Content.Goobstation.Shared.DropPod;
using Content.Server.Radio.EntitySystems;
using Content.Server.Spawners.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.EntityTable;
using Content.Shared.GameTicking.Components;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles;
using Content.Shared.Station.Components;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.StationEvents.DropPod;

public sealed partial class DepartmentDropPodRuleSystem : StationEventSystem<DepartmentDropPodRuleComponent>
{
    [Dependency] private DropPodSystem _dropPod = default!;
    [Dependency] private EntityTableSystem _entityTable = default!;
    [Dependency] private RadioSystem _radio = default!;
    [Dependency] private MetaDataSystem _metaData = default!;

    protected override void Started(EntityUid uid,
        DepartmentDropPodRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var station)
            || !TryComp<StationDataComponent>(station, out var stationData)
            || !TryFindTarget(component, stationData, out var coordinates)
            && !TryFindRandomTileOnStation((station.Value, stationData), out _, out _, out coordinates))
            return;

        _dropPod.Launch(coordinates, _entityTable.GetSpawns(component.Contents));
        SendCommanderMessage(component, coordinates);
    }

    private bool TryFindTarget(DepartmentDropPodRuleComponent component, StationDataComponent station, out EntityCoordinates coordinates)
    {
        coordinates = EntityCoordinates.Invalid;

        var jobs = component.TargetJobs.Count > 0
            ? component.TargetJobs.ToHashSet()
            : PrototypeManager.Index(component.Department).Roles.ToHashSet();

        var grids = new HashSet<EntityUid>(station.Grids);
        var candidates = new List<EntityCoordinates>();
        var query = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        while (query.MoveNext(out var spawn, out var xform))
        {
            if (spawn.SpawnType == SpawnPointType.Job
                && spawn.Job is { } job
                && jobs.Contains(job)
                && xform.GridUid is { } grid
                && grids.Contains(grid))
                candidates.Add(xform.Coordinates);
        }

        if (candidates.Count == 0)
            return false;

        coordinates = RobustRandom.Pick(candidates);
        return true;
    }

    private void SendCommanderMessage(DepartmentDropPodRuleComponent component, EntityCoordinates coordinates)
    {
        if (component.Messages.Count == 0)
            return;

        var speaker = SpawnAtPosition(component.Speaker, coordinates);
        var first = Loc.GetString(RobustRandom.Pick(PrototypeManager.Index(component.FirstNames)));
        var last = Loc.GetString(RobustRandom.Pick(PrototypeManager.Index(component.LastNames)));
        _metaData.SetEntityName(speaker, Loc.GetString(component.SpeakerName, ("first", first), ("last", last)));

        var message = Loc.GetString(RobustRandom.Pick(component.Messages),
            ("problem", Loc.GetString(component.Problem)),
            ("supplies", Loc.GetString(component.Supplies)));

        _radio.SendRadioMessage(speaker, message, component.Channel, speaker);
    }
}
