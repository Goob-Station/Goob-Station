// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Light.Components;
using Content.Server.Mind;
using Content.Server.Pinpointer;
using Content.Server.Power.Components;
using Content.Shared.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Radio.Components;
using Content.Server.Silicons.Laws;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Shared.SurveillanceCamera.Components;
using Content.Server.VoiceMask;
using Content.Goobstation.Shared.MalfunctionAi;
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Chat.RadioIconsEvents;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.VoiceMask;
using Robust.Shared.Player;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Electrocution;
using Content.Shared.Emp;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.StationAi;
using Content.Shared.Turrets;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Popups;
using Content.Shared.RCD.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.Verbs;
using System.Numerics;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.MalfunctionAi;

public sealed partial class MalfunctionAiSystem
{
    private static readonly EntProtoId NightVisionToggleAction = "ActionMalfToggleNightVision";

    private void OnCameraMics(Entity<MalfunctionAiComponent> ent, ref MalfCameraMicsEvent args)
    {
        var mics = EnsureComp<MalfCameraMicsComponent>(ent.Owner);

        // Existing cameras start listening; new cameras are handled on map init.
        var count = 0;
        var query = EntityQueryEnumerator<SurveillanceCameraComponent>();
        while (query.MoveNext(out var camUid, out _))
        {
            EnsureComp<ActiveListenerComponent>(camUid).Range = mics.ListenRange;
            count++;
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-camera-mics-success", ("count", count)), ent.Owner);
    }

    private void OnCameraMapInit(Entity<SurveillanceCameraComponent> ent, ref MapInitEvent args)
    {
        // Newly built cameras inherit the microphone upgrade if it is active. EMP immunity is
        // handled globally in OnCameraEmpAttempt, so nothing extra is needed here for it.
        var micsQuery = EntityQueryEnumerator<MalfCameraMicsComponent>();
        if (micsQuery.MoveNext(out _, out var mics))
            EnsureComp<ActiveListenerComponent>(ent.Owner).Range = mics.ListenRange;

        // Newly built cameras also inherit the see-through-walls vision and extended range once bought.
        var upgradeQuery = EntityQueryEnumerator<MalfCameraUpgradeComponent>();
        if (upgradeQuery.MoveNext(out _, out var upgrade)
            && TryComp<StationAiVisionComponent>(ent.Owner, out var vision))
            _stationAi.SetVisionUpgrade((ent.Owner, vision), false, upgrade.Range);
    }

    /// <summary>
    /// Once any Malfunction AI has upgraded the camera network, its cameras shrug off EMP pulses.
    /// </summary>
    private void OnCameraEmpAttempt(Entity<SurveillanceCameraComponent> ent, ref EmpAttemptEvent args)
    {
        if (EntityManager.Count<MalfCameraUpgradeComponent>() > 0)
            args.Cancelled = true;
    }

    private void OnCameraUpgrade(Entity<MalfunctionAiComponent> ent, ref MalfCameraUpgradeEvent args)
    {
        // Marker: makes the station's cameras EMP-proof (see OnCameraEmpAttempt).
        var upgrade = EnsureComp<MalfCameraUpgradeComponent>(ent.Owner);

        // Let every station camera see through walls and further (clear occlusion + extend range).
        var query = EntityQueryEnumerator<SurveillanceCameraComponent, StationAiVisionComponent>();
        while (query.MoveNext(out var camUid, out _, out var vision))
            _stationAi.SetVisionUpgrade((camUid, vision), false, upgrade.Range);

        // Grant the AI a toggleable night-vision overlay so it can see the station in total darkness.
        // As a mob overlay (not equipment) it toggles the client's lighting off. We add our own
        // access-free toggle action (the AI brain sits in a container and fails the default action's
        // interaction check); it raises the same ToggleNightVisionEvent the overlay expects.
        var nv = EnsureComp<NightVisionComponent>(ent.Owner);

        // Adding the component to an already-map-initialized entity raises MapInitEvent immediately,
        // which makes the overlay system auto-add its default (goggles, non-access-free) toggle action.
        // Strip it so the AI is left with only our access-free action below, not two toggles.
        if (nv.ToggleActionEntity is { } autoAction)
            _actions.RemoveAction(ent.Owner, autoAction);
        nv.ToggleActionEntity = null;

        nv.IsEquipment = false;
        nv.IsActive = false;
        nv.ToggleAction = null;
        Dirty(ent.Owner, nv);

        if (!HasMalfAction(ent.Owner, NightVisionToggleAction))
            _actions.AddAction(ent.Owner, NightVisionToggleAction);

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-camera-upgrade-success"), ent.Owner);
    }

    private void OnCameraListen(Entity<SurveillanceCameraComponent> ent, ref ListenEvent args)
    {
        var camXform = Transform(ent.Owner);

        var query = EntityQueryEnumerator<MalfunctionAiComponent, MalfCameraMicsComponent>();
        while (query.MoveNext(out var aiUid, out _, out var mics))
        {
            // Don't echo the AI's own speech back at it.
            if (args.Source == aiUid)
                continue;

            if (!TryComp<ActorComponent>(aiUid, out var actor))
                continue;

            // The AI's eye must actually be watching near this camera.
            if (!_stationAi.TryGetCore(aiUid, out var core) || core.Comp?.RemoteEntity is not { } eye)
                continue;

            var eyeXform = Transform(eye);
            if (eyeXform.MapID != camXform.MapID)
                continue;

            var distance = (_transform.GetWorldPosition(eyeXform) - _transform.GetWorldPosition(camXform)).Length();
            if (distance > mics.EyeRange)
                continue;

            var wrapped = Loc.GetString("malfunction-ai-camera-mic-relay",
                ("speaker", Name(args.Source)),
                ("message", args.Message));

            _chatManager.ChatMessageToOne(ChatChannel.Local,
                args.Message,
                wrapped,
                ent.Owner,
                false,
                actor.PlayerSession.Channel);
        }
    }
}
