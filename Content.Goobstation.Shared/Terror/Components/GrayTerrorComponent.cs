using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Component used to signify the entity is a gray terror.
/// </summary>

[RegisterComponent, NetworkedComponent]
public sealed partial class GrayTerrorComponent : Component;
