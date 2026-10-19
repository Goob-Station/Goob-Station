// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>

// SPDX-License-Identifier: MIT

using System;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Shared.SurveillanceCamera;
using Content.Shared.Speech.Components;
using Content.Server.SurveillanceCamera;
using Content.Shared.SurveillanceCamera.Components;
using Content.Shared.MalfAI;
using Content.Shared.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Player;

using static Content.Server.Chat.Systems.ChatSystem;

namespace Content.Server.MalfAI;

/// <summary>
/// Server-side logic for the Malf AI "Camera Microphones" upgrade:
/// - Handles the toggle action (starts disabled).
/// - Relays local IC chat (speech/emotes/whispers) to the Malf AI if both:
///   (a) the speaker is within voice range of a microphone-enabled camera, and
///   (b) the AI Eye is within 5 tiles (component-configurable) of that same camera.
/// - Ignores occlusion and supports cross-grid/z-level.
/// - Deduplicates per chat event by only adding the AI recipient once per event.
/// </summary>
public sealed class MalfAiCameraMicrophonesSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly Content.Server.Silicons.StationAi.StationAiSystem _stationAi = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Toggle action handling (only available to entities with StationAiHeldComponent; i.e., the AI).
        SubscribeLocalEvent<StationAiHeldComponent, MalfAiToggleCameraMicrophonesActionEvent>(OnToggle);

        // Add recipients to IC chat when conditions are satisfied.
        SubscribeLocalEvent<ExpandICChatRecipientsEvent>(OnExpandRecipients);
    }

    private void OnToggle(Entity<StationAiHeldComponent> ai, ref MalfAiToggleCameraMicrophonesActionEvent args)
    {
        if (args.Handled)
            return;

        if (ai.Owner == EntityUid.Invalid)
            return;

        var comp = EnsureComp<MalfAiCameraMicrophonesComponent>(ai.Owner);

        // Flip desired state
        comp.EnabledDesired = !comp.EnabledDesired;

        // Compute effective based on core status (we are in-core due to StationAiHeldComponent)
        comp.EnabledEffective = comp.EnabledDesired;

        Dirty(ai.Owner, comp);

        // Show status message to player
        var message = comp.EnabledEffective
            ? Loc.GetString("malfai-camera-microphones-enabled")
            : Loc.GetString("malfai-camera-microphones-disabled");

        if (TryComp(ai.Owner, out ActorComponent? actor))
        {
            // Show popup above AI eye instead of core
            var popupTarget = GetAiEyeForPopup(ai.Owner) ?? ai.Owner;
            _popup.PopupEntity(message, popupTarget, actor.PlayerSession);
        }

        args.Handled = true;
    }

    /// <summary>
    /// Gets the AI eye entity for popup positioning, falls back to core if eye unavailable
    /// </summary>
    private EntityUid? GetAiEyeForPopup(EntityUid aiUid)
    {
        if (!_stationAi.TryGetCore(aiUid, out var core) || core.Comp?.RemoteEntity == null)
            return null;

        return core.Comp.RemoteEntity.Value;
    }

    private void OnExpandRecipients(ExpandICChatRecipientsEvent ev)
    {
        // If the message has no audible range, nothing to do (e.g., non-IC chat).
        // Otherwise ev.VoiceRange is the effective local range (handles whisper/shout automatically).
        var voiceRange = ev.VoiceRange;
        if (voiceRange <= 0f)
            return;

        var xformQuery = GetEntityQuery<TransformComponent>();
        var sourceXform = Transform(ev.Source);
        var sourcePos = _xforms.GetWorldPosition(sourceXform, xformQuery);


        // Iterate all candidate Malf AIs with the upgrade enabled.
        var aiQuery = EntityQueryEnumerator<MalfAiMarkerComponent, StationAiHeldComponent, MalfAiCameraMicrophonesComponent, TransformComponent>();
        while (aiQuery.MoveNext(out var aiUid, out _, out _, out var micComp, out _))
        {
            if (!micComp.EnabledEffective)
                continue;

            // Resolve the AI eye (remote entity).
            if (!_stationAi.TryGetCore(aiUid, out var core) || core.Comp?.RemoteEntity == null)
                continue;

            var eye = core.Comp.RemoteEntity.Value;
            var eyeXform  = Transform(eye);
            var eyePos = _xforms.GetWorldPosition(eyeXform, xformQuery);

            // Find cameras where BOTH the speaker AND the AI eye are in range of the SAME camera.
            var minRangeToSource = float.MaxValue;
            var any = false;

            // Re-enumerate cameras each AI (keeps logic simple; overhead is minimal).
            var camEnum = EntityQueryEnumerator<SurveillanceCameraMicrophoneComponent, ActiveListenerComponent, SurveillanceCameraComponent, TransformComponent>();
            while (camEnum.MoveNext(out var camUid, out _, out _, out var camComp, out var camXform))
            {
                // Only consider cameras that can have viewers (Active true).
                if (!camComp.Active)
                    continue;

                var camPos = _xforms.GetWorldPosition(camXform, xformQuery);

                // AI eye must be within the configured radius (default 5 tiles) of this camera.
                var eyeDist = (camPos - eyePos).Length();
                if (eyeDist > micComp.RadiusTiles)
                    continue;

                // Source must be within the message's local voice range of the same camera.
                var srcDist = (camPos - sourcePos).Length();
                if (srcDist > voiceRange)
                    continue;

                // Both conditions satisfied for this specific camera - AI can hear this speaker
                any = true;
                if (srcDist < minRangeToSource)
                    minRangeToSource = srcDist;
            }

            if (!any)
                continue;

            // Add the AI player's session as a recipient once (dedup automatically via TryAdd).
            if (TryComp(aiUid, out ActorComponent? actor))
            {
                // "As if physically present": add to chat normally (log to chat, not camera-only bubble).
                // Range is the (min) distance from the speaker to the chosen camera to preserve obfuscation behavior.
                // The flags here mirror normal IC delivery rather than camera-view-only injection.
                ev.Recipients.TryAdd(actor.PlayerSession, new ICChatRecipientData(minRangeToSource, true, false));
            }
        }
    }
}
