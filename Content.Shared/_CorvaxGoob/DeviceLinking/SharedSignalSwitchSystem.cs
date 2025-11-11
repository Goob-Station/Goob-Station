using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.DeviceLinking.Systems;

[Serializable, NetSerializable]
public enum SignalSwitchVisuals : byte
{
    State
}

[Serializable, NetSerializable]
public enum SignalSwitchState : byte
{
    On,
    Off
}
