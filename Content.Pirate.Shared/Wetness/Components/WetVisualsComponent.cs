using Robust.Shared.GameStates;

namespace Content.Pirate.Shared.Wetness.Components;

/// <summary>
/// Server-added marker on a wearer while any worn wettable item is above its visual threshold.
/// The client draws a droplet overlay based purely on the presence of this replicated component,
/// kept separate from the stain visuals. Add/remove is server-authoritative.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WetVisualsComponent : Component;
