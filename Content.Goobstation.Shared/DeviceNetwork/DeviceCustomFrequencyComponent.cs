using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.DeviceNetwork;

/// <summary>
/// Allows this device to have a custom frequency that can be edited with a UI interaction.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DeviceCustomFrequencyComponent : Component
{
    /// <summary>
    /// Should Transmit frequency be editable
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool TransmitChange;

    /// <summary>
    /// Should Receive frequency be editable
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ReceiveChange;

    [DataField, AutoNetworkedField]
    public uint MinTransmitFrequency = 1000;

    [DataField, AutoNetworkedField]
    public uint MaxTransmitFrequency = 9999;

    [DataField, AutoNetworkedField]
    public uint MinReceiveFrequency = 1000;

    [DataField, AutoNetworkedField]
    public uint MaxReceiveFrequency = 9999;
}
