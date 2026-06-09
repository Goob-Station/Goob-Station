using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.MapRestricted.Components;

/// <summary>
/// Makes an upgrade only usable on a map that passes a whitelist.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MapRestrictedUpgradeComponent : Component;
