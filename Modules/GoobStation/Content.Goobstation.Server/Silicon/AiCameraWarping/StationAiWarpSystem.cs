// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Silicon.AiCameraWarping;
using Content.Server.Station.Systems;
using Content.Server.SurveillanceCamera;
using Content.Shared.Silicons.StationAi;
using Microsoft.VisualBasic;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Silicon.AiCameraWarping;

public sealed class StationAiWarpSystem : SharedStationAiWarpSystem
{
    [Dependency] private readonly SharedStationAiSystem _stationAiSystem = default!;
    [Dependency] private readonly SharedTransformSystem _xformSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiHeldComponent, CameraWarpActionMessage>(OnUiWarpAction);
        SubscribeLocalEvent<StationAiHeldComponent, CameraWarpRefreshActionMessage>(OnUiRefreshRequested);
        SubscribeLocalEvent<StationAiHeldComponent, ToggleCameraWarpScreenEvent>(ToggleCameraWarpScreen);
    }

    private void OnUiRefreshRequested(Entity<StationAiHeldComponent> ent, ref CameraWarpRefreshActionMessage args)
    {
        if (!_stationAiSystem.TryGetCore(ent.Owner, out var core))
            return;

        var cameras = GetCameras(core.Owner);

        var state = new CameraWarpBuiState(cameras);
        _userInterface.SetUiState(ent.Owner, CamWarpUiKey.Key, state);
    }

    private void ToggleCameraWarpScreen(Entity<StationAiHeldComponent> ent, ref ToggleCameraWarpScreenEvent args)
    {
        if (args.Handled || !TryComp<ActorComponent>(ent.Owner, out var actor))
            return;
        if (!_stationAiSystem.TryGetCore(ent.Owner, out var core))
            return;

        args.Handled = true;

        _userInterface.TryToggleUi(ent.Owner, CamWarpUiKey.Key, actor.PlayerSession);

        var cameras = GetCameras(core.Owner);

        var state = new CameraWarpBuiState(cameras);
        _userInterface.SetUiState(ent.Owner, CamWarpUiKey.Key, state);
    }

    private List<CameraWarpData> GetCameras(EntityUid coreUid)
    {
        List<CameraWarpData> cameras = new();

        var query = EntityManager.EntityQueryEnumerator<SurveillanceCameraComponent>();

        var aiGrid = _xformSystem.GetGrid(coreUid);

        while (query.MoveNext(out var queryUid, out var comp))
        {
            if (_xformSystem.GetGrid(queryUid) != aiGrid || !comp.Active)
                continue;

            var data = new CameraWarpData
            {
                NetEntityUid = GetNetEntity(queryUid),
                DisplayName = comp.CameraId
            };

            cameras.Add(data);
        }

        return cameras;
    }

    private void OnUiWarpAction(Entity<StationAiHeldComponent> ent, ref CameraWarpActionMessage args)
    {
        if (args.WarpAction == null)
            return;

        var target = GetEntity(args.WarpAction.Target);

        // The UI doesn't get refreshed when a camera goes offline or whatever
        // so we gotta check if it can still be jumped to.

        if (!TryComp<SurveillanceCameraComponent>(target, out var camera) || !camera.Active)
            return;

        if (!_stationAiSystem.TryGetCore(ent.Owner, out var core) || core.Comp?.RemoteEntity == null)
            return;

        // The AI shouldn't be able to jump to cams on other stations/shuttles
        if (_xformSystem.GetGrid(core.Owner) != _xformSystem.GetGrid(target))
            return;

        _xformSystem.SetWorldPosition(core.Comp.RemoteEntity.Value, _xformSystem.GetWorldPosition(target));
    }
}