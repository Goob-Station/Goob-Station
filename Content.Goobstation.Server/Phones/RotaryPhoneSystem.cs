using Content.Goobstation.Shared.Phones.Components;
using Content.Server.Speech;
using Content.Shared.Chat;

namespace Content.Goobstation.Server.Phones;

public sealed class RotaryPhoneSystem : EntitySystem
{

    [Dependency] private readonly SharedChatSystem _chatSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RotaryPhoneComponent,ListenEvent>(OnListen);
    }

    private void OnListen(EntityUid uid, RotaryPhoneComponent comp, ListenEvent args)
    {
        if (comp.ConnectedPhone == null)
        {
            var query = EntityQueryEnumerator<RotaryPhoneComponent>();
            while (query.MoveNext(out var phone, out var phoneComp))
            {
                comp.ConnectedPhone = phone;
                break;
            }
        }

        if (comp.ConnectedPhone == null)
            return;

        _chatSystem.TrySendInGameICMessage(comp.ConnectedPhone.Value, args.Message, InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
    }
}
