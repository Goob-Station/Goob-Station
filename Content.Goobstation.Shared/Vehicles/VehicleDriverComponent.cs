using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Vehicles;

/// <summary>
/// A marker component which is attached to entities driving a Vehicle.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleDriverComponent : Component
{
    /// <summary>
    /// The vehicle being driven.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public EntityUid? Vehicle;
}
