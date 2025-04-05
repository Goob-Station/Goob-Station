using Content.Shared.Slippery;
using Content.Shared.Standing;
using Content.Shared.StepTrigger.Components;

namespace Content.Goobstation.Shared.Slippery;

/// <summary>
/// Causes the person given this to inherit
/// Slippery and StepTrigger when they're laying down.
/// </summary>

public sealed class SlipperyOnLayingDownSystem : EntitySystem
{

    [Dependency] private readonly StandingStateSystem _standing = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SlipperyOnLayingDownComponent,DownedEvent>(OnLayingDown);
        SubscribeLocalEvent<SlipperyOnLayingDownComponent,StoodEvent>(OnGetUp);
    }

    private void OnLayingDown(Entity<SlipperyOnLayingDownComponent> uid, ref DownedEvent args)
    {
            EnsureComp<SlipperyComponent>(uid);
            EnsureComp<StepTriggerComponent>(uid);
    }

    private void OnGetUp(Entity<SlipperyOnLayingDownComponent> uid, ref StoodEvent args)
    {
        RemComp<SlipperyComponent>(uid);
        RemComp<StepTriggerComponent>(uid);
    }
}
