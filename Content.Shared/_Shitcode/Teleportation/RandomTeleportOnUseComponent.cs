// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Teleportation;

/// <summary>
///     Entity that will randomly teleport the user when used in hand.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RandomTeleportOnUseComponent : RandomTeleportComponent
{
    /// <summary>
    ///     Whether to consume this item on use; consumes only one if it's a stack
    /// </summary>
    [DataField] public bool ConsumeOnUse = true;
}
