using Robust.Shared.GameStates;

namespace Content.Shared._DV.Whitelist;

/// <summary>
/// Marker component for holofans, used for reclaiming charges of the projector.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HolofanComponent : Component;
