// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Shared._Oskarrr.WorkingJoe;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Mobs;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Oskarrr.WorkingJoe;

public sealed class WorkingJoeVoiceSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    private static readonly string[] DeathSounds =
    [
        "AU14WorkingJoeDeathNormalVar1",
        "AU14WorkingJoeDeathNormalVar2",
        "AU14WorkingJoeDeathTomorrowVar1",
        "AU14WorkingJoeDeathTomorrowVar2",
        "AU14WorkingJoeSilenceVar1",
        "AU14WorkingJoeSilenceVar2",
        "AU14WorkingJoeSilenceVar3",
        "AU14WorkingJoeSilenceVar4",
        "AU14WorkingJoeToSleepPerchanceToDreamVar1"
    ];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WorkingJoeVoiceComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<WorkingJoeVoiceComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<WorkingJoeVoiceComponent, WorkingJoeVoiceActionEvent>(OnAction);
        SubscribeLocalEvent<WorkingJoeVoiceComponent, WorkingJoePlayLineMessage>(OnPlayLine);
        SubscribeLocalEvent<WorkingJoeVoiceComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMapInit(EntityUid uid, WorkingJoeVoiceComponent component, MapInitEvent args)
    {
        _ui.SetUi(uid, WorkingJoeVoiceUiKey.Key, new InterfaceData("WorkingJoeVoiceBui", interactionRange: 0f));
        _actions.AddAction(uid, ref component.ActionEntity, component.Action);
        Dirty(uid, component);
    }

    private void OnShutdown(EntityUid uid, WorkingJoeVoiceComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.ActionEntity);
    }

    private void OnAction(EntityUid uid, WorkingJoeVoiceComponent component, WorkingJoeVoiceActionEvent args)
    {
        _ui.TryToggleUi(uid, WorkingJoeVoiceUiKey.Key, args.Performer);
        args.Handled = true;
    }

    private void OnPlayLine(EntityUid uid, WorkingJoeVoiceComponent component, WorkingJoePlayLineMessage args)
    {
        if (!_proto.TryIndex(args.LineId, out WorkingJoeVoiceLinePrototype? line))
            return;

        var msg = Loc.GetString(line.Name);
        _chat.TrySendInGameICMessage(uid, msg, InGameICChatType.Speak, ChatTransmitRange.Normal);

        var sound = new SoundCollectionSpecifier(line.SoundCollection);
        _audio.PlayPvs(sound, uid);
    }

    private void OnMobStateChanged(EntityUid uid, WorkingJoeVoiceComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var pick = _random.Pick(DeathSounds);
        if (!_proto.HasIndex<SoundCollectionPrototype>(pick))
            return;

        var sound = new SoundCollectionSpecifier(pick, AudioParams.Default.WithVolume(6f));
        _audio.PlayPvs(sound, uid);
    }
}
