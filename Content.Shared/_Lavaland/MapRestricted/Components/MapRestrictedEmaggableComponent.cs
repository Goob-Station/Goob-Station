using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.MapRestricted.Components;

/// <summary>
/// Makes all map restricted components be able to be disabled by an emag.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MapRestrictedEmaggableComponent : Component;
