using Content.Shared.Nutrition;

namespace Content.Goobstation.Shared.Wraith.Other;

public sealed class UnableToEatSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UnableToEatComponent, IngestionAttemptEvent>(OnIngestionAttempt);
    }

    private void OnIngestionAttempt(Entity<UnableToEatComponent> ent, ref IngestionAttemptEvent args)
    {
        // popup here
        args.Cancel();
    }
}
