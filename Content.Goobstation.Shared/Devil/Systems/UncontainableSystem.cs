using Content.Goobstation.Shared.Devil.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Pulling.Events;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Devil.Systems;

/// <summary>
/// Prevents an entity from being inserted into any container or pulled.
/// </summary>
public sealed class UncontainableSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UncontainableComponent, ContainerGettingInsertedAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<UncontainableComponent, BeingPulledAttemptEvent>(OnPullAttempt);
    }

    private void OnInsertAttempt(EntityUid uid, UncontainableComponent component, ContainerGettingInsertedAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnPullAttempt(EntityUid uid, UncontainableComponent component, BeingPulledAttemptEvent args)
    {
        args.Cancel();
    }
}
