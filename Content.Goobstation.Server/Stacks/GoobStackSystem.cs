using Content.Goobstation.Shared.Stacks;
using Content.Server.Stack;
using Content.Shared.Stacks;

namespace Content.Goobstation.Server.Stacks;

/// <summary>
/// This handles...
/// </summary>
public sealed class GoobStackSystem : GoobSharedStackSystem
{

    [Dependency] private readonly StackSystem _stackSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
    }

    // Goobstation - Custom stack splitting dialog
    protected override void OnCustomSplitMessage(Entity<StackComponent> ent, ref StackCustomSplitAmountMessage message)
    {
        var (uid, comp) = ent;

        // digital ghosts shouldn't be allowed to split stacks
        if (!(message.Actor is { Valid: true } user))
            return;

        var amount = message.Amount;
        _stackSystem.UserSplit(uid, user, amount, comp);
    }
}
