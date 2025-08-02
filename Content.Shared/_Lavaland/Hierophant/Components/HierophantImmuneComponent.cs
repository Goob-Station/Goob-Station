using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Hierophant.Components;

/// <summary>
/// Marker component that makes this entity immune to Hierophant's damage tiles.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HierophantImmuneComponent : Component;
