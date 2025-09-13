
using System.Linq;
using Content.Goobstation.Common.Traits;
using Content.Server.Chat.V2;
using Content.Server.Radio;
using Content.Server.Chat;
using Content.Shared.Chat;
using Content.Goobstation.Common.Chat;

namespace Content.Goobstation.Server.Deafness;

public sealed class DeafnessSystem : EntitySystem
{
    private EntityQuery<DeafComponent> _deafQuery;

    public override void Initialize()
    {
        base.Initialize();

        _deafQuery = GetEntityQuery<DeafComponent>();
        SubscribeLocalEvent<RadioReceiveAttemptEvent>(OnRadioReceiveAttempt);
        SubscribeLocalEvent<DeafComponent, ChatMessageOverrideInVoiceRange>(OnOverrideInVoiceRange);
    }

    private void OnOverrideInVoiceRange(EntityUid uid, DeafComponent comp, ref ChatMessageOverrideInVoiceRange args)  // blocks normal chat
    {
        args.Cancel();
    }

    private void OnRadioReceiveAttempt(ref RadioReceiveAttemptEvent args) // blocks radio
    {
        var user = Transform(args.RadioReceiver).ParentUid;

        if (!_deafQuery.HasComp(user))
            return;

        args.Cancelled = true;
    }
}
