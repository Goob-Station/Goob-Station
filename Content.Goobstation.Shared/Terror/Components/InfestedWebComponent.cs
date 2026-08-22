using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Component used to signify the entity is an infested web.
/// </summary>

[RegisterComponent, NetworkedComponent]
public sealed partial class InfestedWebComponent : Component;
