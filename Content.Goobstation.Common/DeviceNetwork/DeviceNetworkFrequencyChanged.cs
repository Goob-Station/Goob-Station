namespace Content.Goobstation.Common.DeviceNetwork;

[ByRefEvent]
public record struct DeviceNetworkReceiveFrequencyChangedEvent(uint? OldFrequency, uint? NewFrequency);

[ByRefEvent]
public record struct DeviceNetworkTransmitFrequencyChangedEvent(uint? OldFrequency, uint? NewFrequency);
