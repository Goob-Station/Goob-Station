using Content.Goobstation.Server.Doodons;
using Content.Goobstation.Shared.Doodon.Objectives;
using Content.Goobstation.Shared.Doodons; // for DoodonComponent, DoodonTownHallComponent, DoodonBuildingComponent
using Content.Server.Objectives.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Objectives.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Doodon.Objectives;

public sealed class DoodonObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KeepWorkersAliveConditionComponent, ObjectiveGetProgressEvent>(OnGetKeepWorkersAliveProgress);
        SubscribeLocalEvent<BuildBuildingsConditionComponent, ObjectiveGetProgressEvent>(OnGetBuildBuildingsProgress);
    }

    private void OnGetKeepWorkersAliveProgress(EntityUid uid, KeepWorkersAliveConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);
        if (target == 0)
        {
            args.Progress = 1f;
            return;
        }

        var alive = CountAliveWorkers(comp.WorkerPrototype);
        args.Progress = MathF.Min(alive / (float) target, 1f);
    }

    private int CountAliveWorkers(EntProtoId workerProto)
    {
        var count = 0;

        var query = EntityQueryEnumerator<MobStateComponent, MetaDataComponent>();
        while (query.MoveNext(out var _, out var mobState, out var meta))
        {
            if (mobState.CurrentState == MobState.Dead)
                continue;

            if (meta.EntityPrototype?.ID != workerProto.Id)
                continue;

            count++;
        }

        return count;
    }

    private void OnGetBuildBuildingsProgress(EntityUid uid, BuildBuildingsConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);
        if (target == 0)
        {
            args.Progress = 1f;
            return;
        }

        // MindComponent is already args.Mind
        var owned = args.Mind.OwnedEntity;
        if (owned == null || Deleted(owned.Value))
        {
            args.Progress = 0f;
            return;
        }

        if (!TryComp<DoodonComponent>(owned.Value, out var doodon) || doodon.TownHall == null)
        {
            args.Progress = 0f;
            return;
        }

        var hallUid = doodon.TownHall.Value;

        if (!TryComp<DoodonTownHallComponent>(hallUid, out var hallComp))
        {
            args.Progress = 0f;
            return;
        }

        var connectedBuildings = CountConnectedBuildings(hallUid, hallComp);
        args.Progress = MathF.Min(connectedBuildings / (float) target, 1f);
    }

    private int CountConnectedBuildings(EntityUid hallUid, DoodonTownHallComponent hall)
    {
        var connected = 0;

        foreach (var b in hall.Buildings)
        {
            if (Deleted(b))
                continue;

            if (!TryComp<DoodonBuildingComponent>(b, out var building))
                continue;

            if (!building.Active)
                continue;

            if (building.TownHall != hallUid)
                continue;

            connected++;
        }

        return connected;
    }
}
