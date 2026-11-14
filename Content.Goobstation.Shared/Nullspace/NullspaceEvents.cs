using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Nullspace;

[Serializable, NetSerializable]
public sealed class NullspaceLookEvent(float yaw, float pitch) : EntityEventArgs
{
    public float Yaw = yaw;
    public float Pitch = pitch;
}

[Serializable, NetSerializable]
public sealed class NullspaceStartEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class NullspaceSilenceEvent(float seconds) : EntityEventArgs
{
    public float Seconds = seconds;
}
