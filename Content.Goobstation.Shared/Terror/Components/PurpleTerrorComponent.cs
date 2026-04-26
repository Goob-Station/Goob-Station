using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Marks this entity as a purple terror spider, filtering it into
/// <see cref="TerrorProximitySystem"/> for leash consequence handling.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PurpleTerrorComponent : Component;
