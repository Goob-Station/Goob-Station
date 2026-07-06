using Robust.Shared.GameStates;

namespace Content.Pirate.Shared.Wetness.Components;

/// <summary>
/// Marker for the client-side droplet overlay.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WetVisualsComponent : Component;
