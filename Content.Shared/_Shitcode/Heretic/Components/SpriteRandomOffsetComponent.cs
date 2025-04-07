using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpriteRandomOffsetComponent : Component
{
    [DataField]
    public float MinX = -0.25f;

    [DataField]
    public float MaxX = 0.25f;

    [DataField]
    public float MinY = -0.25f;

    [DataField]
    public float MaxY = 0.25f;
}

[Serializable, NetSerializable]
public enum OffsetVisuals : byte
{
    Offset
}
