using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Magic;

/// <summary>
///     Indicates an entity that can use magic.
///     Isn't supposed to do anything by itself.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MagicUserComponent : Component
{
}
