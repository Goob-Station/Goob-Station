using Content.Goobstation.Shared.Devil.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Devil.Systems;

/// <summary>
/// This system is used to prevent an entity from being put into any container or from being pulled.
/// </summary>
public sealed class UncontainableSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<UncontainableComponent, ContainerGettingInsertedAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<UncontainableComponent, MapInitEvent>(OnMapInit);
    }
    private void OnInsertAttempt(EntityUid uid, UncontainableComponent component, ContainerGettingInsertedAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnMapInit(EntityUid uid, UncontainableComponent component, MapInitEvent args)
    {
        if (TryComp<PullableComponent>(uid, out var pullable))
        {
            pullable.PreventPulling = true;
            Dirty(uid, pullable);
        }
    }

}
