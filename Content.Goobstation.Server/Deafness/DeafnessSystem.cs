using System.Linq;
using Content.Goobstation.Shared.Traits.Components;
using Content.Server.Chat.V2;
using Content.Server.Radio;
using Content.Shared.Chat;
using Content.Server.Chat;

namespace Content.Goobstation.Server.Deafness;

public sealed class DeafnessSystem : EntitySystem
{
    private EntityQuery<DeafComponent> _deafQuery;

    public override void Initialize()
    {
        base.Initialize();

        _deafQuery = GetEntityQuery<DeafComponent>();
        SubscribeLocalEvent<RadioReceiveAttemptEvent>(OnRadioReceiveAttempt);
        SubscribeLocalEvent<DeafComponent, ChatMessageOverrideInVoiceRangeEvent>(OnOverrideInVoiceRange);
    }

    private void OnOverrideInVoiceRange(EntityUid uid, DeafComponent comp, ref ChatMessageOverrideInVoiceRangeEvent args)  // blocks normal chat
    {
        if (args.Channel is ChatChannel.Emotes 
            or ChatChannel.Damage
            or ChatChannel.Visual
            or ChatChannel.Notifications
            or ChatChannel.OOC
            or ChatChannel.LOOC)
            return;

        args.Cancelled = true;
    }

    private void OnRadioReceiveAttempt(ref RadioReceiveAttemptEvent args) // blocks radio
    {
        var user = Transform(args.RadioReceiver).ParentUid;

        if (!_deafQuery.HasComp(user))
            return;

        args.Cancelled = true;
    }
}
