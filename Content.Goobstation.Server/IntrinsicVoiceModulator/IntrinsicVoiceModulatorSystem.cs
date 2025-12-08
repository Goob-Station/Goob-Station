// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Goobstation.Shared.IntrinsicVoiceModulator; // Goobstation
using Content.Goobstation.Shared.IntrinsicVoiceModulator.Components; // Goobstation
using Content.Goobstation.Shared.IntrinsicVoiceModulator.Events; // Goobstation
using Content.Shared.Administration.Logs;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Chat.RadioIconsEvents; // Goobstation
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.IntrinsicVoiceModulator;

public sealed partial class IntrinsicVoiceModulatorSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;

    private int _maxNameLenght;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IntrinsicVoiceModulatorComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<IntrinsicVoiceModulatorComponent, TransformSpeakerNameEvent>(OnTransformSpeakerName);
        SubscribeLocalEvent<IntrinsicVoiceModulatorComponent, TransformSpeakerJobIconEvent>(OnTransformJobIcon);
        SubscribeLocalEvent<IntrinsicVoiceModulatorComponent, OpenIntrinsicVoiceModulatorMenuEvent>(OnOpenVoiceModulatorMenu);

        Subs.BuiEvents<IntrinsicVoiceModulatorComponent>(IntrinsicVoiceModulatorUiKey.Key,
            subs =>
        {
            subs.Event<IntrinsicVoiceModulatorNameChangedMessage>(OnNameChangedMessage);
            subs.Event<IntrinsicVoiceModulatorJobIconChangedMessage>(OnJobIconChanged);
            subs.Event<IntrinsicVoicemodulatorVerbChangedMessage>(OnVerbChangeMessage);
        });

        Subs.CVar(_cfg, CCVars.MaxNameLength, value => _maxNameLenght = value, true);
    }

    private void OnComponentInit(Entity<IntrinsicVoiceModulatorComponent> ent, ref ComponentInit args)
    {
        var data = new InterfaceData("IntrinsicVoiceModulatorBoundUserInterface");
        _ui.SetUi(ent.Owner, IntrinsicVoiceModulatorUiKey.Key, data);
    }

    private void OnTransformSpeakerName(Entity<IntrinsicVoiceModulatorComponent> ent, ref TransformSpeakerNameEvent args)
    {

        if (!string.IsNullOrWhiteSpace(ent.Comp.VoiceName))
            args.VoiceName = ent.Comp.VoiceName;

        if (ent.Comp.SpeechVerbProtoId is { } speechVerb)
            args.SpeechVerb = speechVerb;
    }

    private void OnTransformJobIcon(Entity<IntrinsicVoiceModulatorComponent> ent, ref TransformSpeakerJobIconEvent args)
    {

        if (ent.Comp.JobIconProtoId is { } jobIcon)
            args.JobIcon = jobIcon;

        if (!string.IsNullOrWhiteSpace(ent.Comp.JobName))
            args.JobName = ent.Comp.JobName;
    }

    private void OnOpenVoiceModulatorMenu(Entity<IntrinsicVoiceModulatorComponent> ent, ref OpenIntrinsicVoiceModulatorMenuEvent ev)
    {
        if (!_ui.HasUi(ev.Performer, IntrinsicVoiceModulatorUiKey.Key))
            return;

        _ui.OpenUi(ev.Performer, IntrinsicVoiceModulatorUiKey.Key, ev.Performer);
    }

    private void OnNameChangedMessage(Entity<IntrinsicVoiceModulatorComponent> ent, ref IntrinsicVoiceModulatorNameChangedMessage args)
    {
        if (args.Name.Length > _maxNameLenght
            || args.Name.Length == 0)
        {
            _popup.PopupEntity(Loc.GetString("intrinsic-voice-modulator-popup-failure"), ent, args.Actor, PopupType.SmallCaution);
            return;
        }

        ent.Comp.VoiceName = args.Name;

        _adminLog.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(args.Actor):player} set them voice: {ent.Comp.VoiceName}");

        _popup.PopupEntity(Loc.GetString("intrinsic-voice-modulator-popup-success"), ent, args.Actor);
        UpdateUi(ent);
    }

    private void OnVerbChangeMessage(Entity<IntrinsicVoiceModulatorComponent> ent, ref IntrinsicVoicemodulatorVerbChangedMessage args)
    {
        if (args.SpeechProtoId is not { } speechProtoId
            || !_proto.HasIndex(speechProtoId))
            return;

        ent.Comp.SpeechVerbProtoId = speechProtoId;

        _popup.PopupEntity(Loc.GetString("intrinsic-voice-modulator-popup-success"), ent, args.Actor);
        UpdateUi(ent);
    }

    private void OnJobIconChanged(Entity<IntrinsicVoiceModulatorComponent> ent, ref IntrinsicVoiceModulatorJobIconChangedMessage args)
    {
        if (!_proto.TryIndex(args.JobIconProtoId, out var proto)
            || !proto.AllowSelection)
            return;

        ent.Comp.JobIconProtoId = proto.ID;

        if (_job.TryFindJobFromIcon(proto, out var job))
            ent.Comp.JobName = job.LocalizedName;
        else
            ent.Comp.JobName = null;

        _popup.PopupEntity(Loc.GetString("intrinsic-voice-modulator-popup-success"), ent, args.Actor);
        UpdateUi(ent);
    }

    private void UpdateUi(Entity<IntrinsicVoiceModulatorComponent> ent)
    {
        var (uid, comp) = ent;

        if (!_ui.IsUiOpen(uid, IntrinsicVoiceModulatorUiKey.Key))
            return;

        var buiState = new IntrinsicVoiceModulatorBoundUserInterfaceState(comp.VoiceName, comp.SpeechVerbProtoId, comp.JobIconProtoId);

        _ui.SetUiState(uid, IntrinsicVoiceModulatorUiKey.Key, buiState);
    }
}
