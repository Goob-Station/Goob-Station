using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Tools;

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
}
