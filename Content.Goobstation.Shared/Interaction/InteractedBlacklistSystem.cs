using Content.Shared.Interaction.Events;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Interaction;

// taken out of RMCInteractionSystem and removed the light thing
public sealed class InteractedBlacklistSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InteractedBlacklistComponent, GettingInteractedWithAttemptEvent>(OnBlacklistInteractionAttempt);
    }

    private void OnBlacklistInteractionAttempt(Entity<InteractedBlacklistComponent> ent, ref GettingInteractedWithAttemptEvent args)
    {
        if (args.Cancelled || ent.Comp.Blacklist == null)
            return;

        if (_whitelist.IsValid(ent.Comp.Blacklist, args.Uid))
            args.Cancelled = true;
    }
}
