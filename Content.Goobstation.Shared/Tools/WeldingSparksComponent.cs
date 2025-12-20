using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Tools;

/// <summary>
/// Creates an effect when a tool with this component is used on an entity.
/// </summary>
[RegisterComponent]
public sealed partial class WeldingSparksComponent : Component
{
    /// <summary>
    /// Prototype of the effect to spawn. (Defaults to <c>EffectWeldingSparks</c>)
    /// </summary>
    [DataField("effect")]
    public EntProtoId EffectProto = "EffectWeldingSparks";

    /// <summary>
    /// Dictionary of currently active welding spark effects spawned by this component,
    /// indexed by the <see cref="DoAfterId"/> of the DoAfter that triggered them.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<DoAfterId, EntityUid> SpawnedEffects = [];

    /// <summary>
    /// The last recorded click location by a user of the tool.
    /// </summary>
    public EntityCoordinates? LastClickLocation;
}
