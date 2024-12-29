namespace Content.Server.Teleportation;

/// <summary>
///     Entity that will randomly teleport the user when used in hand.
/// </summary>
[RegisterComponent]
public sealed partial class RandomTeleportOnUseComponent : RandomTeleportComponent
{
    /// <summary>
    ///     Whether to consume this item on use; consumes only one if it's a stack
    /// </summary>
    [DataField] public bool ConsumeOnUse = true;
}
