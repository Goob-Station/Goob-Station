using Robust.Shared.GameStates;

namespace Content.Shared.DeltaV.VendingMachines;

/// <summary>
/// Makes a <see cref="ShopVendorComponent"/> use mining points to buy items.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PointsVendorComponent : Component;
