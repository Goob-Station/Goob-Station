using Robust.Shared.GameStates;

/// <summary>
/// Marks this step trap as a terror web, routing it into
/// <see cref="TerrorWebSystem"/> for terror-specific effect handling.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TerrorWebComponent : Component;
