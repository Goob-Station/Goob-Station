using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Flammability;

/// <summary>
///     Indicates that, when on fire, it should ignore all fire protection.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class VeryFlammableComponent : Component { }
