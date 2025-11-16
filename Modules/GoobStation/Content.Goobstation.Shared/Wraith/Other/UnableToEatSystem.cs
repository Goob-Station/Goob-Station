using Content.Shared.Nutrition;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Wraith.Other;

public sealed class UnableToEatSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UnableToEatComponent, IngestionAttemptEvent>(OnIngestionAttempt);
    }

    private void OnIngestionAttempt(Entity<UnableToEatComponent> ent, ref IngestionAttemptEvent args)
    {
        _popup.PopupEntity(Loc.GetString("curse-rot-cant-eat"), ent.Owner, ent.Owner);
        args.Cancel();
    }
}
