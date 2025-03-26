using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Common.DoAfter;

[RegisterComponent]
public sealed partial class DoAfterDelayMultiplierComponent : Component
{
    [DataField]
    public float Multiplier = 1f;
}
