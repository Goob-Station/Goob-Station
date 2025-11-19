using Content.Shared.Actions.Events;

namespace Content.Goobstation.Shared.Werewolf;

/// <summary>
/// deletes the action on use (why isnt it a thing already)
/// </summary>
public sealed class DeleteActionOnPerformedSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DisposableActionComponent, ActionPerformedEvent>(OnAction);
    }

    private void OnAction(Entity<DisposableActionComponent> ent, ref ActionPerformedEvent args)
    {
        QueueDel(ent);
    }
}
