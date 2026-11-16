using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Jiggle;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JiggleEmoteEvent : EntityEventArgs
{
    [DataField(required: true)]
    public List<JigglePulse> Pulses = new();
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JigglePulse
{
    [DataField]
    public float Delay;

    [DataField(required: true)]
    public Vector2 Impulse;
}
