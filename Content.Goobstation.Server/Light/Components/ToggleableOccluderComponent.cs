using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Light.Components;

/// <summary>
///     Allows entities with OccluderComponent to toggle that component on and off, early upstream merge of #30743
/// </summary>
[RegisterComponent]
public sealed partial class ToggleableOccluderComponent : Component
{
    /// <summary>
    ///     Port for toggling occluding on.
    /// </summary>
    [DataField]
    public ProtoId<SinkPortPrototype> OnPort = "On";

    /// <summary>
    ///     Port for toggling occluding off.
    /// </summary>
    [DataField]
    public ProtoId<SinkPortPrototype> OffPort = "Off";

    /// <summary>
    ///     Port for toggling occluding.
    /// </summary>
    [DataField]
    public ProtoId<SinkPortPrototype> TogglePort = "Toggle";
}
