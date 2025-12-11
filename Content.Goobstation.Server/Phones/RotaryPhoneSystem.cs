using Content.Goobstation.Shared.Phones.Components;
using Content.Server.Chat.Managers;
using Content.Server.Speech;
using Content.Shared.Chat;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Phones;

public sealed class RotaryPhoneSystem : EntitySystem
{

    [Dependency] private readonly SharedChatSystem _chatSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RotaryPhoneComponent,ListenEvent>(OnListen);
    }

    private void OnListen(EntityUid uid, RotaryPhoneComponent comp, ref ListenEvent args)
    {
        if(HasComp<RotaryPhoneComponent>(args.Source) || !_timing.IsFirstTimePredicted || args.Source == uid)
            return;

        if (comp.ConnectedPhone == null)
        {
            var query = EntityQueryEnumerator<RotaryPhoneComponent>();
            while (query.MoveNext(out var phone, out var phoneComp))
            {
                if (comp.DialedNumber == phoneComp.PhoneNumber)
                {
                    comp.ConnectedPhone = phone;
                    break;
                }
            }
        }

        if (comp.ConnectedPhone == null)
            return;

        _chatSystem.TrySendInGameICMessage(comp.ConnectedPhone.Value, args.Message, InGameICChatType.Speak, hideChat: true, hideLog: true, checkRadioPrefix: false);
    }
}
