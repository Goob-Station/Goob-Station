using Content.Goobstation.Shared.Phones.Components;
using Content.Goobstation.Shared.Phones.Events;
using Content.Server.Chat.Managers;
using Content.Server.Radio.Components;
using Content.Server.Speech;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Phones;

public sealed class RotaryPhoneSystem : EntitySystem
{

    [Dependency] private readonly SharedChatSystem _chatSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RotaryPhoneComponent, ListenEvent>(OnListen);
    }

    private void OnListen(EntityUid uid, RotaryPhoneComponent comp, ref ListenEvent args)
    {
        if(HasComp<RotaryPhoneComponent>(args.Source) || !_timing.IsFirstTimePredicted || args.Source == uid || HasComp<RadioSpeakerComponent>(args.Source))
            return;

        if (comp.ConnectedPhone == null)
        {
            var query = EntityQueryEnumerator<RotaryPhoneComponent>();
            while (query.MoveNext(out var phone, out var phoneComp))
            {
                if (comp.DialedNumber == phoneComp.PhoneNumber)
                {
                    if (!phoneComp.Engaged)
                    {
                        comp.Engaged = true;
                        phoneComp.Engaged = true;
                        var audio = _audio.PlayPvs(comp.RingingSound, uid, AudioParams.Default.WithLoop(true));
                        if (audio != null)
                            comp.SoundEntity = audio.Value.Entity;

                        RaiseLocalEvent(phone, new PhoneRingEvent(uid, comp));
                    }
                    else
                    {
                        var audio = _audio.PlayPvs(comp.HandUpSoundLocal, uid);
                        if (audio != null)
                            comp.SoundEntity = audio.Value.Entity;
                    }
                    break;
                }
            }
        }

        if (comp.ConnectedPhone == null || !comp.Connected)
            return;

        _chatSystem.TrySendInGameICMessage(comp.ConnectedPhone.Value, args.Message, InGameICChatType.Speak, hideChat: true, hideLog: true, checkRadioPrefix: false);
    }
}
