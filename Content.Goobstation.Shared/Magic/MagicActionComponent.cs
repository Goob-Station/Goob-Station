using Content.Shared.Chat;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Magic;

[RegisterComponent, NetworkedComponent]
public sealed partial class MagicActionComponent : Component
{
    /// <summary>
    ///     What will the user say on invocation?
    ///     If this is not empty, it will run an additional speech check.
    /// </summary>
    [DataField] public LocId? InvocationLoc = null;

    /// <summary>
    ///     How will the user say <see cref="InvocationLoc"/>?
    /// </summary>
    [DataField] public InGameICChatType InvocationType = InGameICChatType.Speak;

    /// <summary>
    ///     How much specific clothing pieces/items/focuses are required for the spell to work?
    /// </summary>
    [DataField] public int RequiredMagicalItemWeight = 0;
}
