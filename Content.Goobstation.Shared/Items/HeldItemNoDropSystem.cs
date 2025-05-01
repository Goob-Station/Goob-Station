using Content.Goobstation.Common.Items;

namespace Content.Goobstation.Shared.Items;

public sealed class HeldItemNoDropSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeldItemNoDropComponent, AttemptDropHeldItemEvent>(OnAttempt);
    }

    private void OnAttempt(Entity<HeldItemNoDropComponent> ent, ref AttemptDropHeldItemEvent args)
    {
        args.Cancel();
    }
}
