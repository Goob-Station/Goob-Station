using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Augments;

public abstract class SharedAugmentToolPanelSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentToolPanelActiveItemComponent, ContainerGettingRemovedAttemptEvent>(OnDropAttempt);
    }

    private void OnDropAttempt(Entity<AugmentToolPanelActiveItemComponent> ent, ref ContainerGettingRemovedAttemptEvent args)
    {
        // you can never drop an active tool panel item, it has to be retracted with the action
        if (!_timing.ApplyingState)
            args.Cancel();
    }
}
