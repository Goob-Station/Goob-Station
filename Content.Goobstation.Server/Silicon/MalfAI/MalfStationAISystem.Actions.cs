// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.Silicon.MalfAI.Rules;
using Content.Goobstation.Shared.Silicon.MalfAI;
using Content.Goobstation.Shared.Silicon.MalfAI.Components;
using Content.Goobstation.Shared.Silicon.MalfAI.Events;
using Content.Server.Doors.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Station.Systems;
using Content.Server.SurveillanceCamera;
using Content.Server.Wires;
using Content.Shared.Doors.Components;
using Content.Shared.Electrocution;
using Content.Shared.Explosion.Components;
using Content.Shared.Explosion.Components.OnTrigger;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Station;
using Content.Shared.StationAi;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.Silicon.MalfAI;

public sealed partial class MalfStationAISystem : SharedMalfStationAISystem
{
    [Dependency] private readonly SharedStationAiSystem _stationAi = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly DoorSystem _door = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly WiresSystem _wires = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public void InitializeActions()
    {
        SubscribeLocalEvent<MalfStationAIComponent, MachineOverloadActionEvent>(OnOverloadAction);
        SubscribeLocalEvent<MalfStationAIComponent, HostileLockdownActionEvent>(OnLockdownEvent);

        SubscribeLocalEvent<MalfStationAIComponent, UpgradeCamerasActionEvent>(OnCameraUpgrade);
        SubscribeLocalEvent<MalfStationAIComponent, ReactivateCameraActionEvent>(OnReactivateCamera);

        SubscribeLocalEvent<MalfStationAIComponent, DoomsDayActionEvent>(OnDoomsDayStart);
    }

    private void OnDoomsDayStart(Entity<MalfStationAIComponent> ent, ref DoomsDayActionEvent args)
    {
        var gamerule = EntityQuery<MalfAIRuleComponent>().First();

        // The AI must be on-station in order to activate the device.
        if (!IsAIAliveAndOnStation(ent, gamerule.Station))
        {
            _popup.PopupEntity(Loc.GetString(gamerule.OnlyOnStationLoc), ent, ent, Content.Shared.Popups.PopupType.LargeCaution);
            return;
        }

        // There can only be one dooms day device active at a time.
        if (gamerule.DoomDeviceActive)
        {
            _popup.PopupEntity(Loc.GetString(gamerule.OnlyOneDeviceLoc), ent, ent, Content.Shared.Popups.PopupType.LargeCaution);
            return;
        }

        StartDoomsDayDevice(ent);

        args.Handled = true;
    }

    private void OnReactivateCamera(Entity<MalfStationAIComponent> ent, ref ReactivateCameraActionEvent args)
    {
        if (!_stationAi.TryGetCore(ent, out var core) || core.Comp?.RemoteEntity == null)
            return;

        var query = _lookup.GetEntitiesInRange<SurveillanceCameraComponent>(Transform(core.Comp.RemoteEntity.Value).Coordinates, ent.Comp.CameraRepairRadius, LookupFlags.Static);

        foreach (var cam in query)
        {
            if (ReactivateCamera(cam))
            {
                args.Handled = true;
                return;
            }
        }
    }

    /// <summary>
    /// Mend all of this camera's wires.
    /// Does not repair damage to the camera.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public bool ReactivateCamera(Entity<SurveillanceCameraComponent> entity)
    {
        var camRepaired = false;

        if (!TryComp<WiresComponent>(entity, out var wiresComp))
            return false;

        var wires = _wires.TryGetWires<Wire>(entity, wiresComp);

        foreach (var wire in wires)
        {
            if (!wire.IsCut || wire.Action == null || !wire.Action.Mend(entity, wire))
                continue;

            wire.IsCut = false;

            camRepaired = true;
        }

        return camRepaired;
    }

    private void OnCameraUpgrade(Entity<MalfStationAIComponent> ent, ref UpgradeCamerasActionEvent args)
    {
        if (!_stationAi.TryGetCore(ent, out var core))
            return;

        args.Handled = true;

        var query = EntityQueryEnumerator<StationAiVisionComponent, TransformComponent>();

        var gridUid = _xform.GetGrid(Transform(core).Coordinates);

        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            // If its not on the same station as the AI then skip it.
            if (_xform.GetGrid(xform.Coordinates) != gridUid)
                continue;

            _stationAi.SetVisionOcclusion((uid, comp), false);
        }

        if (!TryComp<EyeComponent>(ent, out var eye))
            return;

        // Disable light drawing
        // to give night vision.
        _eye.SetDrawLight((ent.Owner, eye), false);
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