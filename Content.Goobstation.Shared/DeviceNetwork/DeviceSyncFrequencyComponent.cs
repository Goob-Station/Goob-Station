using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.DeviceNetwork;

/// <summary>
/// Forces a device to have the same synchronized frequency as some other source device when it changes.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DeviceSyncFrequencyComponent : Component
{

}
