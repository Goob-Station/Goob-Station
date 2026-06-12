using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components;

/// <summary>
/// Added to the vampire while olfaction tracking is active.
/// Triggers the red vision overlay on the client.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerOlfactionOverlayComponent : Component;
