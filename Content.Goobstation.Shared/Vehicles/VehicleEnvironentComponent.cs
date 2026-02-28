using Content.Shared.Atmos;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Vehicles;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedVehicleSystem))]
public sealed partial class VehicleEnvironmentComponent : Component, IGasMixtureHolder
{
    /// <summary>
    ///     If false, vehicle will generate an infinite (specified) gas environment to its driver.
    /// </summary>
    [DataField]
    public bool RequireTank = false;

    /// <summary>
    ///     If RequireTank field is true, the attached gas tank entity used to breathe from. This
    ///     is via the inventoryComponent of the vehicle.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? GasTankEntity;

    /// <summary>
    ///     If RequireTank field is false, the volume of gas initilised in the vehicle.
    /// </summary>
    public const float GasMixVolume = 70f;

    /// <summary>
    ///     If RequireTank field is false, the generated air mixture for the occupant to breathe.
    /// </summary>
    [DataField("air")]
    public GasMixture Air { get; set; } = new(GasMixVolume);
}
