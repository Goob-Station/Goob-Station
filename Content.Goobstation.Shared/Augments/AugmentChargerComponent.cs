using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Augments;

/// <summary>
/// Marker component to indicate that an entity serves as an AugmentCharger organ
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentChargerComponent : Component;

/// <summary>
/// Marker component to indicate that an entity will recharge its augment power cell from borg charging stations
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentStationRechargeComponent : Component;

