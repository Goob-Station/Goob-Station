using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.DeviceNetwork;

[Serializable, NetSerializable]
public enum DeviceCustomFrequencyUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class DeviceCustomReceiveFrequencyChangeMessage : BoundUserInterfaceMessage
{
    public uint ReceiveFrequency { get; }

    public DeviceCustomReceiveFrequencyChangeMessage(uint receiveFrequency)
    {
        ReceiveFrequency = receiveFrequency;
    }
}

[Serializable, NetSerializable]
public sealed class DeviceCustomTransmitFrequencyChangeMessage : BoundUserInterfaceMessage
{
    public uint TransmitFrequency { get; }

    public DeviceCustomTransmitFrequencyChangeMessage(uint transmitFrequency)
    {
        TransmitFrequency = transmitFrequency;
    }
}

[Serializable, NetSerializable]
public sealed class DeviceCustomResetFrequencyMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class DeviceCustomFrequencyUserInterfaceState : BoundUserInterfaceState
{
    public uint? ReceiveFrequency;
    public uint? TransmitFrequency;

    public DeviceCustomFrequencyUserInterfaceState(uint? receiveFrequency, uint? transmitFrequency)
    {
        ReceiveFrequency = receiveFrequency;
        TransmitFrequency = transmitFrequency;
    }
}
