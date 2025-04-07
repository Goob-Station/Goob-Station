using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
///     Used for identifying other changelings.
///     Indicates that a changeling has bought the hivemind access ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HivemindComponent : Component
{
}
