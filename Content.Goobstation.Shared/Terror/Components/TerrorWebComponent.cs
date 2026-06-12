using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Marks this step trap as a terror web, routing it into
/// <see cref="TerrorWebSystem"/> for terror-specific effect handling.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TerrorWebComponent : Component;
