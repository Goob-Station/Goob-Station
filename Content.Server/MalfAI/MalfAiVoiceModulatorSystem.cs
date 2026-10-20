// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Silicons.StationAi;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Chat;
using Content.Shared.Chat.RadioIconsEvents;
using Content.Shared.Database;
using Content.Shared.MalfAI;
using Content.Shared.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.MalfAI;

public sealed class MalfAiVoiceModulatorSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Open the Malf voice modulator UI on action use.
        SubscribeLocalEvent<MalfAiVoiceModulatorActionEvent>(OnVoiceModulator);

        // Receive state update or toggle from client.
        SubscribeNetworkEvent<MalfVoiceModulatorUpdateEvent>(OnModulatorUpdate);

        // Name spoofing on radio / chat.
        SubscribeLocalEvent<MalfVoiceModulatorComponent, TransformSpeakerNameEvent>(OnTransformSpeakerName);
        SubscribeLocalEvent<StationAiCoreComponent, TransformSpeakerNameEvent>(OnTransformSpeakerNameCore);
        SubscribeLocalEvent<StationAiEyeComponent, TransformSpeakerNameEvent>(OnTransformSpeakerNameEye);

        // Radio job icon & title spoofing.
        SubscribeLocalEvent<MalfVoiceModulatorComponent, TransformSpeakerJobIconEvent>(OnTransformJobIcon);
        SubscribeLocalEvent<StationAiCoreComponent, TransformSpeakerJobIconEvent>(OnTransformJobIconCore);
        SubscribeLocalEvent<StationAiEyeComponent, TransformSpeakerJobIconEvent>(OnTransformJobIconEye);

        // Clean up any dynamic accent components when modulator component is removed.
        SubscribeLocalEvent<MalfVoiceModulatorComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnVoiceModulator(MalfAiVoiceModulatorActionEvent ev)
    {
        if (!TryComp<ActorComponent>(ev.Performer, out var actor))
            return;

        var ai = ResolveAiEntity(ev.Performer);
        if (ai == EntityUid.Invalid)
            return;

        var modulator = EnsureComp<MalfVoiceModulatorComponent>(ai);

        RaiseNetworkEvent(new MalfVoiceModulatorOpenUiEvent(
            modulator.Enabled,
            modulator.VoiceName,
            modulator.JobProtoId,
            modulator.Accent),
            Filter.SinglePlayer(actor.PlayerSession));
    }

    private void OnModulatorUpdate(MalfVoiceModulatorUpdateEvent ev, EntitySessionEventArgs args)
    {
        var performer = args.SenderSession.AttachedEntity;
        if (performer == null)
            return;

        var ai = ResolveAiEntity(performer.Value);
        if (ai == EntityUid.Invalid)
            return;

        var modulator = EnsureComp<MalfVoiceModulatorComponent>(ai);
        var oldEnabled = modulator.Enabled;

        modulator.Enabled = ev.Enabled;
        modulator.VoiceName = ev.VoiceName.Trim();

        // Job selection from HoP list
        if (ev.JobProtoId != null && _proto.TryIndex<JobPrototype>(ev.JobProtoId, out var jobProto))
        {
            modulator.JobProtoId = jobProto.ID;
            modulator.JobIconProtoId = jobProto.Icon;
            modulator.JobName = jobProto.LocalizedName;
        }
        else
        {
            modulator.JobProtoId = null;
            modulator.JobIconProtoId = null;
            modulator.JobName = null;
        }

        // Speech accent transformation
        SetAccent(ai, modulator, ev.Accent);

        Dirty(ai, modulator);

        var popupTarget = GetAiEyeForPopup(ai) ?? ai;
        if (oldEnabled != modulator.Enabled)
        {
            var msg = modulator.Enabled ? "malf-voice-popup-enabled" : "malf-voice-popup-disabled";
            _popup.PopupEntity(Loc.GetString(msg), popupTarget, performer.Value);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("malf-voice-updated"), popupTarget, performer.Value);
        }

        _adminLog.Add(LogType.Action, LogImpact.Medium,
            $"AI voice modulator updated for {ToPrettyString(ai)}: Enabled={modulator.Enabled}, Name=\"{modulator.VoiceName}\", Job={modulator.JobProtoId}, Accent={modulator.Accent}");

        RaiseNetworkEvent(new MalfVoiceModulatorStateEvent(
            modulator.Enabled,
            modulator.VoiceName,
            modulator.JobProtoId,
            modulator.Accent),
            Filter.SinglePlayer(args.SenderSession));
    }

    private void SetAccent(EntityUid ai, MalfVoiceModulatorComponent modulator, string? newAccent)
    {
        // If an accent component was previously added by the modulator, remove it.
        if (modulator.ActiveAccentComponent != null)
        {
            if (Factory.TryGetRegistration(modulator.ActiveAccentComponent, out var reg))
            {
                RemComp(ai, reg.Type);
            }
            modulator.ActiveAccentComponent = null;
        }

        modulator.Accent = newAccent;

        // If enabled and an accent is specified, add the new accent component.
        if (modulator.Enabled && !string.IsNullOrEmpty(newAccent))
        {
            if (Factory.TryGetRegistration(newAccent, out var reg))
            {
                var comp = (Component) Factory.GetComponent(reg.Type);
                AddComp(ai, comp);
                modulator.ActiveAccentComponent = newAccent;
            }
        }
    }

    private void OnShutdown(Entity<MalfVoiceModulatorComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.ActiveAccentComponent != null &&
            Factory.TryGetRegistration(ent.Comp.ActiveAccentComponent, out var reg))
        {
            RemComp(ent.Owner, reg.Type);
            ent.Comp.ActiveAccentComponent = null;
        }
    }

    private void OnTransformSpeakerName(Entity<MalfVoiceModulatorComponent> ent, ref TransformSpeakerNameEvent args)
    {
        if (!ent.Comp.Enabled)
            return;

        if (!string.IsNullOrWhiteSpace(ent.Comp.VoiceName))
            args.VoiceName = ent.Comp.VoiceName;
    }

    private void OnTransformSpeakerNameCore(Entity<StationAiCoreComponent> ent, ref TransformSpeakerNameEvent args)
    {
        if (_stationAi.TryGetHeld((ent.Owner, ent.Comp), out var held) &&
            TryComp<MalfVoiceModulatorComponent>(held, out var mod) &&
            mod.Enabled &&
            !string.IsNullOrWhiteSpace(mod.VoiceName))
        {
            args.VoiceName = mod.VoiceName;
        }
    }

    private void OnTransformSpeakerNameEye(Entity<StationAiEyeComponent> ent, ref TransformSpeakerNameEvent args)
    {
        if (_stationAi.TryGetCore(ent.Owner, out var core) &&
            _stationAi.TryGetHeld(core, out var held) &&
            TryComp<MalfVoiceModulatorComponent>(held, out var mod) &&
            mod.Enabled &&
            !string.IsNullOrWhiteSpace(mod.VoiceName))
        {
            args.VoiceName = mod.VoiceName;
        }
    }

    private void OnTransformJobIcon(Entity<MalfVoiceModulatorComponent> ent, ref TransformSpeakerJobIconEvent args)
    {
        if (!ent.Comp.Enabled)
            return;

        if (ent.Comp.JobIconProtoId != null)
            args.JobIcon = ent.Comp.JobIconProtoId.Value;

        if (!string.IsNullOrWhiteSpace(ent.Comp.JobName))
            args.JobName = ent.Comp.JobName;
    }

    private void OnTransformJobIconCore(Entity<StationAiCoreComponent> ent, ref TransformSpeakerJobIconEvent args)
    {
        if (_stationAi.TryGetHeld((ent.Owner, ent.Comp), out var held) &&
            TryComp<MalfVoiceModulatorComponent>(held, out var mod) &&
            mod.Enabled)
        {
            if (mod.JobIconProtoId != null)
                args.JobIcon = mod.JobIconProtoId.Value;

            if (!string.IsNullOrWhiteSpace(mod.JobName))
                args.JobName = mod.JobName;
        }
    }

    private void OnTransformJobIconEye(Entity<StationAiEyeComponent> ent, ref TransformSpeakerJobIconEvent args)
    {
        if (_stationAi.TryGetCore(ent.Owner, out var core) &&
            _stationAi.TryGetHeld(core, out var held) &&
            TryComp<MalfVoiceModulatorComponent>(held, out var mod) &&
            mod.Enabled)
        {
            if (mod.JobIconProtoId != null)
                args.JobIcon = mod.JobIconProtoId.Value;

            if (!string.IsNullOrWhiteSpace(mod.JobName))
                args.JobName = mod.JobName;
        }
    }

    private EntityUid ResolveAiEntity(EntityUid performer)
    {
        if (HasComp<StationAiHeldComponent>(performer))
            return performer;

        if (_stationAi.TryGetCore(performer, out var core) && _stationAi.TryGetHeld(core, out var held))
            return held.Value;

        var resolvedCore = SharedMalfAiHelpers.ResolveAiCoreFrom(EntityManager, _xform, performer);
        if (resolvedCore != EntityUid.Invalid &&
            TryComp<StationAiCoreComponent>(resolvedCore, out var coreComp) &&
            _stationAi.TryGetHeld((resolvedCore, coreComp), out var heldBrain))
        {
            return heldBrain.Value;
        }

        return performer;
    }

    /// <summary>
    /// Gets the AI eye entity for popup positioning, falls back to core if eye unavailable.
    /// </summary>
    private EntityUid? GetAiEyeForPopup(EntityUid aiUid)
    {
        if (!_stationAi.TryGetCore(aiUid, out var core) || core.Comp?.RemoteEntity == null)
            return null;

        return core.Comp.RemoteEntity.Value;
    }
}
