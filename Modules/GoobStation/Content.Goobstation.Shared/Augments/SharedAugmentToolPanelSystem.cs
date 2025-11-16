using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Augments;

public abstract class SharedAugmentToolPanelSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentToolPanelActiveItemComponent, ContainerGettingRemovedAttemptEvent>(OnDropAttempt);
    }

    private void OnDropAttempt(Entity<AugmentToolPanelActiveItemComponent> ent, ref ContainerGettingRemovedAttemptEvent args)
    {
        // you can never drop an active tool panel item, it has to be retracted with the action
        args.Cancel();
    }
}
