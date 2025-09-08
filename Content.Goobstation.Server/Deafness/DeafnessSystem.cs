using System.Linq;
using Content.Goobstation.Shared.Traits.Components;
using Content.Server.Chat.V2;
using Content.Server.Radio;
using Content.Shared.Chat;
using Content.Server.Chat;

namespace Content.Goobstation.Server.Deafness;

public sealed class DeafnessSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RadioReceiveAttemptEvent>(OnRadioReceiveAttempt);
        SubscribeLocalEvent<DeafComponent, ChatMessageOverrideInVoiceRangeEvent>(OnOverrideInVoiceRange);
    }

    private void OnOverrideInVoiceRange(EntityUid uid, DeafComponent comp, ref ChatMessageOverrideInVoiceRangeEvent args)  // blocks normal chat
    {
        if (args.Channel == ChatChannel.Emotes
            || args.Channel == ChatChannel.Damage
            || args.Channel == ChatChannel.Visual
            || args.Channel == ChatChannel.Notifications
            || args.Channel == ChatChannel.OOC
            || args.Channel == ChatChannel.LOOC)
            return;

        args.Cancelled = true;
    }

    private void OnRadioReceiveAttempt(ref RadioReceiveAttemptEvent args) // blocks radio
    {
        var user = Transform(args.RadioReceiver).ParentUid;

        if (!HasComp<DeafComponent>(user))
            return;

        args.Cancelled = true;
        Logger.Debug(args.Cancelled.ToString());
    }
}