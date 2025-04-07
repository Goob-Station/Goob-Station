using Content.Shared.Stacks;

namespace Content.Goobstation.Shared.Stacks;

/// <summary>
/// This handles...
/// </summary>
public abstract class GoobSharedStackSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<StackComponent, StackCustomSplitAmountMessage>(OnCustomSplitMessage); // Goobstation - Custom stack splitting dialog
    }

     // Custom stack splitting dialog
     // client shouldn't try to split stacks so do nothing on client
    protected virtual void OnCustomSplitMessage(Entity<StackComponent> ent, ref StackCustomSplitAmountMessage message) {}
}
