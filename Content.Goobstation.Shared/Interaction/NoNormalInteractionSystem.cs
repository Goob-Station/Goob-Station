using Content.Shared.Interaction.Events;

namespace Content.Goobstation.Shared.Interaction;

public sealed class NoNormalInteractionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NoNormalInteractionComponent, InteractionAttemptEvent>(OnInteractionAttempt);
    }

    private void OnInteractionAttempt(EntityUid uid, NoNormalInteractionComponent component, InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }
}
