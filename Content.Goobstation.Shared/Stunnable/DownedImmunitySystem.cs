using Content.Shared.Stunnable;

namespace Content.Goobstation.Shared.Stunnable;

public sealed class DownedImmunitySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DownedImmunityComponent, KnockDownAttemptEvent>(OnKnockDownAttempt);
    }

    private void OnKnockDownAttempt(Entity<DownedImmunityComponent> ent, ref KnockDownAttemptEvent args)
    {
        args.Cancelled = true;
    }
}
