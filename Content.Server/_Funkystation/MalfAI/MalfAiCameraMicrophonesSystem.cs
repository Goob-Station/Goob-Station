// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Actions;
using Content.Server.Chat.Systems;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Server.SurveillanceCamera;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Player;
using static Content.Server.Chat.Systems.ChatSystem;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Server-side logic for the Malf AI "Camera Microphones" upgrade:
/// relays local IC chat to the Malf AI when both the speaker is within voice range
/// of a microphone-equipped camera and the AI eye is near that same camera.
/// </summary>
public sealed class MalfAiCameraMicrophonesSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiCameraMicrophonesUnlockedEvent>(OnCameraMicrophonesUnlocked);
        SubscribeLocalEvent<ExpandICChatRecipientsEvent>(OnExpandRecipients);
        SubscribeLocalEvent<MalfAiCameraMicrophonesComponent, MalfAiToggleCameraMicrophonesActionEvent>(OnToggle);
    }

    private void OnToggle(Entity<MalfAiCameraMicrophonesComponent> ent, ref MalfAiToggleCameraMicrophonesActionEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.EnabledDesired = !ent.Comp.EnabledDesired;
        ent.Comp.EnabledEffective = ent.Comp.EnabledDesired;
        Dirty(ent);

        var key = ent.Comp.EnabledDesired ? "malfai-camera-microphones-enabled" : "malfai-camera-microphones-disabled";
        _popup.PopupEntity(Loc.GetString(key), ent.Owner, ent.Owner);
        args.Handled = true;
    }

    private void OnCameraMicrophonesUnlocked(Entity<MalfAiMarkerComponent> ent, ref MalfAiCameraMicrophonesUnlockedEvent args)
    {
        _actions.AddAction(ent.Owner, "ActionMalfAiToggleCameraMicrophones");

        // Ensure the per-AI microphones component exists and mark it enabled by default.
        var comp = EnsureComp<MalfAiCameraMicrophonesComponent>(ent.Owner);
        comp.EnabledDesired = true;
        comp.EnabledEffective = true;
        Dirty(ent.Owner, comp);
    }

    private void OnExpandRecipients(ExpandICChatRecipientsEvent ev)
    {
        // If the message has no audible range, nothing to do (e.g., non-IC chat).
        var voiceRange = ev.VoiceRange;
        if (voiceRange <= 0f)
            return;

        var xformQuery = GetEntityQuery<TransformComponent>();
        if (!TryComp<TransformComponent>(ev.Source, out var sourceXform))
            return;

        var sourcePos = _xforms.GetWorldPosition(sourceXform, xformQuery);

        // Iterate all candidate Malf AIs with the upgrade enabled.
        var aiQuery = EntityQueryEnumerator<MalfAiMarkerComponent, StationAiHeldComponent, MalfAiCameraMicrophonesComponent, TransformComponent>();
        while (aiQuery.MoveNext(out var aiUid, out _, out _, out var micComp, out _))
        {
            if (!micComp.EnabledEffective)
                continue;

            // Resolve the AI eye (remote entity of the holding core).
            var core = Transform(aiUid).ParentUid;
            if (!TryComp<StationAiCoreComponent>(core, out var coreComp) || coreComp.RemoteEntity is not { } eye)
                continue;

            if (!TryComp<TransformComponent>(eye, out var eyeXform))
                continue;

            var eyePos = _xforms.GetWorldPosition(eyeXform, xformQuery);

            // Find cameras where BOTH the speaker AND the AI eye are in range of the SAME camera.
            var minRangeToSource = float.MaxValue;
            var any = false;

            // Most cameras have no SurveillanceCameraMicrophone component, so any active camera can listen.
            var camEnum = EntityQueryEnumerator<SurveillanceCameraComponent, TransformComponent>();
            while (camEnum.MoveNext(out _, out var camComp, out var camXform))
            {
                if (!camComp.Active)
                    continue;

                var camPos = _xforms.GetWorldPosition(camXform, xformQuery);

                // AI eye must be within the configured radius of this camera.
                if ((camPos - eyePos).Length() > micComp.RadiusTiles)
                    continue;

                // Speaker must be within the message's voice range of the same camera.
                var srcDist = (camPos - sourcePos).Length();
                if (srcDist > voiceRange)
                    continue;

                any = true;
                if (srcDist < minRangeToSource)
                    minRangeToSource = srcDist;
            }

            if (!any)
                continue;

            // Add the AI player's session as a recipient once.
            if (TryComp<ActorComponent>(aiUid, out var actor))
            {
                ev.Recipients.TryAdd(actor.PlayerSession, new ICChatRecipientData(minRangeToSource, false));
            }
        }
    }
}
