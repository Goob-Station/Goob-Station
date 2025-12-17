using System.Linq;
using Content.Goobstation.Shared.Phones.Components;
using Content.Goobstation.Shared.Phones.Events;
using Content.Server.Chat.Managers;
using Content.Server.Radio.Components;
using Content.Server.Speech;
using Content.Shared.Audio;
using Content.Shared.Chat;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Phones;

public sealed class RotaryPhoneSystem : EntitySystem
{

    [Dependency] private readonly SharedChatSystem _chatSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RotaryPhoneComponent, ListenEvent>(OnListen);
        SubscribeLocalEvent<RotaryPhoneComponent, PhoneKeypadMessage>(OnKeyPadPressed);
        SubscribeLocalEvent<RotaryPhoneComponent, PhoneKeypadClearMessage>(OnKeyPadClear);
        SubscribeLocalEvent<RotaryPhoneComponent, PhoneBookPressedMessage>(OnPhoneBookButtonPressed);
        SubscribeLocalEvent<RotaryPhoneComponent, PhoneDialedMessage>(OnDial);
        SubscribeLocalEvent<RotaryPhoneComponent, BoundUIOpenedEvent>(OnOpen);
    }

    private void OnOpen(EntityUid uid, RotaryPhoneComponent component, BoundUIOpenedEvent args)
    {
        var state = new GoobPhoneBuiState(GetAllPhoneData());
        _ui.SetUiState(uid, PhoneUiKey.Key, state);
    }

    private List<PhoneData> GetAllPhoneData()
    {
        var data = new List<PhoneData>();
        var query = EntityQueryEnumerator<RotaryPhoneComponent, TransformComponent>();

        while (query.MoveNext(out var phone, out var phoneComp, out var xform))
        {
            if (xform.MapID == MapId.Nullspace)
                continue;

            if (phoneComp.PhoneNumber == null || phoneComp.Category == null)
                continue;

            var phones = new PhoneData(phoneComp.Name ?? "Unknown", phoneComp.Category, phoneComp.PhoneNumber.Value);

            data.Add(phones);
        }

        return data;
    }

    private void OnPhoneBookButtonPressed(EntityUid uid, RotaryPhoneComponent comp, PhoneBookPressedMessage args)
    {
        comp.DialedNumber = args.Value;
        Dirty(uid, comp);
    }

    private void OnKeyPadPressed(EntityUid uid, RotaryPhoneComponent comp, PhoneKeypadMessage args)
    {
        PlayPhoneSound(uid, args.Value, comp);
        comp.DialedNumber = (comp.DialedNumber ?? 0) * 10 + args.Value;
        Dirty(uid, comp);
    }

    private void OnKeyPadClear(EntityUid uid, RotaryPhoneComponent comp, PhoneKeypadClearMessage args)
    {
        comp.DialedNumber = null;
        Dirty(uid, comp);
    }
    private void PlayPhoneSound(EntityUid uid, int number, RotaryPhoneComponent? component = null) // Stolen from nuke code
    {
        if (!Resolve(uid, ref component))
            return;

        var semitoneShift = number - 2;

        var opts = component.KeypadPressSound.Params;
        opts = AudioHelpers.ShiftSemitone(opts, semitoneShift).AddVolume(-7f);
        _audio.PlayPvs(component.KeypadPressSound, uid, opts);
    }

    private void OnDial(EntityUid uid, RotaryPhoneComponent comp, PhoneDialedMessage args)
    {
        if (comp.ConnectedPhone == null)
        {
            var query = EntityQueryEnumerator<RotaryPhoneComponent>();
            while (query.MoveNext(out var phone, out var phoneComp))
            {
                if (comp.DialedNumber == phoneComp.PhoneNumber && phone != uid)
                {
                    if (!phoneComp.Engaged)
                    {
                        comp.Engaged = true;
                        comp.ConnectedPhone = phone;
                        phoneComp.Engaged = true;
                        var audio = _audio.PlayPvs(comp.RingingSound, uid, AudioParams.Default.WithLoop(true));
                        if (audio != null)
                            comp.SoundEntity = audio.Value.Entity;

                        RaiseLocalEvent(phone, new PhoneRingEvent(uid, comp));
                    }
                    else if(comp.SoundEntity == null)
                    {
                        var audio = _audio.PlayPvs(comp.BusySound, uid);
                        if (audio != null)
                            comp.SoundEntity = audio.Value.Entity;
                    }
                    break;
                }
            }
        }
        Dirty(uid, comp);
    }

    private void OnListen(EntityUid uid, RotaryPhoneComponent comp, ref ListenEvent args)
    {
        if(HasComp<RotaryPhoneComponent>(args.Source) || !_timing.IsFirstTimePredicted || args.Source == uid || HasComp<RadioSpeakerComponent>(args.Source) || comp.ConnectedPhone == null || !comp.Connected || !TryComp(comp.ConnectedPhone, out RotaryPhoneComponent? otherPhoneComponent))
            return;

        var entityMeta = MetaData(args.Source);

        if (otherPhoneComponent.SpeakerPhone)
        {
            _chatSystem.TrySendInGameICMessage(comp.ConnectedPhone.Value,
                args.Message,
                InGameICChatType.Speak,
                hideChat: true,
                hideLog: true,
                checkRadioPrefix: false,
                nameOverride: entityMeta.EntityName);

            return;
        }

        if(!TryComp(comp.ConnectedPlayer, out ActorComponent? actor) || otherPhoneComponent.ConnectedPlayer == null)
            return;

        var sound = _audio.GetSound(comp.SpeakSound);

        var message = Loc.GetString("phone-speak", ("name", entityMeta.EntityName), ("message", args.Message));

        _chatManager.ChatMessageToOne(ChatChannel.Local, message, message, otherPhoneComponent.ConnectedPlayer.Value, false, actor.PlayerSession.Channel, Color.FromHex("#9956D3"), true, sound, -12, hidePopup: true);
    }
}
