using Content.Goobstation.Shared.Devil.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Popups;
using Content.Shared.Pulling.Events;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Devil.Systems;

/// <summary>
/// Prevents an entity from being inserted into any container or pulled.
/// </summary>
public sealed class UncontainableSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UncontainableComponent, ContainerGettingInsertedAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<UncontainableComponent, BeingPulledAttemptEvent>(OnPullAttempt);
    }

    private void OnInsertAttempt(EntityUid uid, UncontainableComponent component, ContainerGettingInsertedAttemptEvent args)
    {
        _popup.PopupPredicted(Loc.GetString("uncontainable-insert-attempt", ("name", uid)), args.Container.Owner, args.Container.Owner, PopupType.LargeCaution);
        args.Cancel();
    }

    private void OnPullAttempt(EntityUid uid, UncontainableComponent component, BeingPulledAttemptEvent args)
    {
        _popup.PopupPredicted(Loc.GetString("uncontainable-pull-attempt", ("name", uid)), args.Puller, args.Puller, PopupType.LargeCaution);
        args.Cancel();
    }
}
