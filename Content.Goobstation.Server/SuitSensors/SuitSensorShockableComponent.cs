using Content.Shared.Medical.SuitSensor;

namespace Content.Goobstation.Server.SuitSensors;

/// <summary>
/// If on an entity that has SuitSensorComponent, and the entity that wears it gets electrocuted, the sensors will change.
/// </summary>
[RegisterComponent]
public sealed partial class SuitSensorShockableComponent : Component
{
    /// <summary>
    /// List of sensor modes that can be randomly selected when electrocuted, this prob should not be empty
    /// </summary>
    [DataField]
    public List<SuitSensorMode> AvailableModes = new()
    {
        SuitSensorMode.SensorOff,
        SuitSensorMode.SensorBinary,
        SuitSensorMode.SensorVitals,
        SuitSensorMode.SensorCords
    };
}
