// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates; // Goobstation

namespace Content.Goobstation.Common.Religion;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BibleUserComponent : Component
{
    /// <summary>
    /// Bound Nullrod
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? NullRod;
}
