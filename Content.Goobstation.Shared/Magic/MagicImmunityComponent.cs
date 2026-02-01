using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Magic;

/// <summary>
///     Indicates that an entity or it's parent is immune to magic.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MagicImmunityComponent : Component
{

}
