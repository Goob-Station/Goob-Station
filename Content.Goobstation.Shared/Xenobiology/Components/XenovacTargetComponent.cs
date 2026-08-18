using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Components;

/// <summary>
/// Marker component for mobs that can be sucked up by a xenovac.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class XenovacTargetComponent : Component;
