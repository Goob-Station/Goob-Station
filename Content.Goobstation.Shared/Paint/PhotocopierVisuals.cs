using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Paint;

[Serializable, NetSerializable]
public enum PhotocopierVisuals : byte
{
    State,
}

[Serializable, NetSerializable]
public enum PhotocopierState : byte
{
    Idle,
    Scanning,
    Printing,
}
