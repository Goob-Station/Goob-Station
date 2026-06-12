// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Speech.Components;
using Content.Shared.Chat.RadioIconsEvents;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared._Funkystation.MalfAI.VoiceModulator;
using Robust.Shared.Player;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Allows the Malf AI to change its displayed voice name.
/// Opens a UI to choose a voice name, then applies it via VoiceOverrideComponent.
/// </summary>
public sealed class MalfAiVoiceModulatorSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiVoiceModulatorActionEvent>(OnOpenVoiceModulator);
        SubscribeNetworkEvent<MalfVoiceModulatorSubmitNameEvent>(OnSubmitName);
        SubscribeLocalEvent<MalfAiVoiceComponent, TransformSpeakerJobIconEvent>(OnTransformJobIcon);
    }

    private void OnOpenVoiceModulator(Entity<MalfAiMarkerComponent> ent, ref MalfAiVoiceModulatorActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<ActorComponent>(ent.Owner, out var actor))
            return;

        RaiseNetworkEvent(new MalfVoiceModulatorOpenUiEvent(), actor.PlayerSession);
        args.Handled = true;
    }

    private void OnSubmitName(MalfVoiceModulatorSubmitNameEvent ev, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;
        if (session.AttachedEntity is not { } uid)
            return;

        if (!HasComp<MalfAiMarkerComponent>(uid))
            return;

        if (string.IsNullOrWhiteSpace(ev.Name) || ev.Name.Length > 50)
            return;

        // VoiceMaskComponent is meant for worn clothing items; VoiceOverride changes
        // the voice of the entity itself, which is what we want for the AI.
        // The speech verb is chosen by the player in the UI, like the voice mask.
        var voiceOverride = EnsureComp<VoiceOverrideComponent>(uid);
        voiceOverride.NameOverride = ev.Name;
        voiceOverride.SpeechVerbOverride = ev.SpeechVerb;
        voiceOverride.Enabled = true;

        // Radio job icon disguise, like the voice mask (GabyStation radio icons).
        var voice = EnsureComp<MalfAiVoiceComponent>(uid);
        voice.JobIcon = ev.JobIcon;
    }

    private void OnTransformJobIcon(Entity<MalfAiVoiceComponent> ent, ref TransformSpeakerJobIconEvent args)
    {
        if (ent.Comp.JobIcon is not { } icon)
            return;

        args.JobIcon = icon;
        // No reliable job name for a disguised icon: hide the tooltip.
        args.JobName = null;
    }
}
