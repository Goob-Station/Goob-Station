using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Lets an entity wrap dead humanoids into a cocoon.
/// </summary>
[RegisterComponent]
public sealed partial class TerrorWrapComponent : Component
{
    [DataField(required: true)]
    public EntProtoId CocoonProto;

    [DataField]
    public TimeSpan DoAfter = TimeSpan.FromSeconds(5);
}
