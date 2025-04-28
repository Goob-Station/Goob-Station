using Content.Server.Singularity.Events;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.EventHorizon;

public sealed class EventHorizonIgnoreSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EventHorizonIgnoreComponent, EventHorizonAttemptConsumeEntityEvent>(OnAttemptConsume);
    }

    private void OnAttemptConsume(Entity<EventHorizonIgnoreComponent> ent, ref EventHorizonAttemptConsumeEntityEvent args)
    {
        args.Cancelled = args.Cancelled || _whitelist.IsValid(ent.Comp.HorizonWhitelist, args.EventHorizonUid);
    }
}
