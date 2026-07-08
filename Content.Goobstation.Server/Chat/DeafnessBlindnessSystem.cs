
using System.Linq;
using Content.Goobstation.Common.Traits;
using Content.Server.Chat.V2;
using Content.Server.Radio;
using Content.Server.Chat;
using Content.Shared.Chat;
using Content.Goobstation.Common.Chat;
using Content.Shared.Traits.Assorted;
using Content.Shared.Eye.Blinding.Components;

namespace Content.Goobstation.Server.Deafness;

public sealed class DeafnessBlindnessSystem : EntitySystem
{
    private EntityQuery<DeafComponent> _deafQuery;

    public override void Initialize()
    {
        base.Initialize();

        _deafQuery = GetEntityQuery<DeafComponent>();
        SubscribeLocalEvent<RadioReceiveAttemptEvent>(OnDeafnessRadioReceiveAttempt);
        SubscribeLocalEvent<DeafComponent, ChatMessageOverrideInRange>(OnDeafnessOverrideInRange);

        SubscribeLocalEvent<TemporaryBlindnessComponent, ChatMessageOverrideInRange>(OnBlindnessOverrideInRange);
        SubscribeLocalEvent<PermanentBlindnessComponent, ChatMessageOverrideInRange>(OnBlindnessOverrideInRange);
    }

    private void OnDeafnessOverrideInRange(EntityUid uid, DeafComponent comp, ref ChatMessageOverrideInRange args)  // blocks normal chat
    {
        if (!args.RequiresHearing)
            return;
        args.Cancel();
    }

    private void OnDeafnessRadioReceiveAttempt(ref RadioReceiveAttemptEvent args) // blocks radio
    {
        var user = Transform(args.RadioReceiver).ParentUid;

        if (!_deafQuery.HasComp(user))
            return;

        args.Cancelled = true;
    }

    private void OnBlindnessOverrideInRange(Entity<TemporaryBlindnessComponent> ent, ref ChatMessageOverrideInRange args)
    {
        if (!args.RequiresSight)
            return;
        args.Cancel();
    }
    private void OnBlindnessOverrideInRange(Entity<PermanentBlindnessComponent> ent, ref ChatMessageOverrideInRange args)
    {
        if (!args.RequiresSight)
            return;
        args.Cancel();
    }
}
