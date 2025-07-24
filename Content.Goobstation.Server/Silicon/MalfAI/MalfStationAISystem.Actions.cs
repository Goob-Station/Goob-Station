using System.Linq;
using Content.Goobstation.Shared.Silicon.MalfAI;
using Content.Goobstation.Shared.Silicon.MalfAI.Components;
using Content.Goobstation.Shared.Silicon.MalfAI.Events;
using Content.Server.Doors.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.Components;
using Content.Server.SurveillanceCamera;
using Content.Shared._Goobstation.Wizard.Traps;
using Content.Shared.Doors.Components;
using Content.Shared.Electrocution;
using Content.Shared.Explosion.Components;
using Content.Shared.Explosion.Components.OnTrigger;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StationAi;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.Silicon.MalfAI;

public sealed partial class MalfStationAISystem : SharedMalfStationAISystem
{
    [Dependency] private readonly SharedStationAiSystem _stationAi = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly DoorSystem _door = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public void InitializeActions()
    {
        SubscribeLocalEvent<MalfStationAIComponent, MachineOverloadActionEvent>(OnOverloadAction);
        SubscribeLocalEvent<MalfStationAIComponent, HostileLockdownActionEvent>(OnLockdownEvent);

        SubscribeLocalEvent<MalfStationAIComponent, UpgradeCamerasActionEvent>(OnCameraUpgrade);
        SubscribeLocalEvent<MalfStationAIComponent, ReactivateCameraActionEvent>(OnRepairCamera);
    }

    private void OnRepairCamera(Entity<MalfStationAIComponent> ent, ref ReactivateCameraActionEvent args)
    {
        if (!_stationAi.TryGetCore(ent, out var core) || core.Comp?.RemoteEntity == null)
            return;

        var query = _lookup.GetEntitiesInRange<SurveillanceCameraComponent>(Transform(core.Comp.RemoteEntity.Value).Coordinates, ent.Comp.CameraRepairRadius, LookupFlags.Static);

        var camRepaired = false;

        foreach (var cam in query)
        {
            if (!TryComp<StationAiVisionComponent>(cam, out var visionComp) || visionComp.Enabled)
                continue;

            // This should really mend the wire but eh, they can mend and cut the wire aftward to break it again.
            _stationAi.SetVisionEnabled((cam.Owner, visionComp), true, true);

            camRepaired = true;
        }

        args.Handled = camRepaired;
    }

    private void OnCameraUpgrade(Entity<MalfStationAIComponent> ent, ref UpgradeCamerasActionEvent args)
    {
        if (_stationAi.TryGetCore(ent, out var core))
            return;

        var query = EntityQueryEnumerator<StationAiVisionComponent, TransformComponent>();

        var gridUid = _xform.GetGrid(Transform(core).Coordinates);

        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (_xform.GetGrid(xform.Coordinates) != gridUid)
                continue;

            _stationAi.SetVisionOcclusion((uid, comp), false);
        }

        args.Handled = true;
    }

    private void OnLockdownEvent(Entity<MalfStationAIComponent> ent, ref HostileLockdownActionEvent args)
    {
        var aiGrid = _xform.GetGrid(Transform(ent).Coordinates);

        LockdownStation(aiGrid);

        args.Handled = true;
    }

    public void LockdownStation(EntityUid? gridUid)
    {
        var airlockQuery = EntityQueryEnumerator<DoorBoltComponent, DoorComponent, TransformComponent>();

        while (airlockQuery.MoveNext(out var airlockUid, out var bolt, out var door, out var xform))
        {
            if (_xform.GetGrid(xform.Coordinates) != gridUid)
                continue;

            if (door.State == DoorState.Open)
            {
                _door.StartClosing(airlockUid, door);
                _door.Crush(airlockUid, door);
            }
            else
                _door.SetBoltsDown((airlockUid, bolt), true);

            if (!TryComp<ElectrifiedComponent>(airlockUid, out var electrified))
                continue;

            electrified.Enabled = true;
        }
    }

    private void OnOverloadAction(Entity<MalfStationAIComponent> ent, ref MachineOverloadActionEvent args)
    {
        if (HasComp<ActiveTimerTriggerComponent>(args.Target))
            return;

        if (!TryComp<ApcPowerReceiverComponent>(args.Target, out var machine))
            return;

        if (!machine.Powered)
            return;

        // Explosive hack begin.

        // Basically you *can* add an explosive comp to an entity BUT
        // you can't change any of the variables outside of the ExplosiveSystem SO
        // I'm storing a "default" explosive component on the action entity itself and
        // just copying that over. 

        if (!TryComp<ExplosiveComponent>(args.Action, out var explosive))
            return;

        CopyComp(args.Action, args.Target, explosive);

        // Explosive hack end.

        EnsureComp<ExplosiveComponent>(args.Target);
        EnsureComp<ExplodeOnTriggerComponent>(args.Target);

        _trigger.HandleTimerTrigger(args.Target, ent, ent.Comp.SecondsToOverload, 1.0f, 0.0f, ent.Comp.BuzzingSound);

        args.Handled = true;
    }
}