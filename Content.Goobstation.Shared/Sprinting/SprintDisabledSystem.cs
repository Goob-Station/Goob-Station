namespace Content.Goobstation.Shared.Sprinting;

/// <summary>
/// Blocks sprint attempts.
/// </summary>
public sealed class SprintDisabledSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SprintDisabledComponent, SprintAttemptEvent>(OnSprintAttempt);
    }

    private void OnSprintAttempt(Entity<SprintDisabledComponent> ent, ref SprintAttemptEvent args)
    {
        args.Cancel();
    }
}
