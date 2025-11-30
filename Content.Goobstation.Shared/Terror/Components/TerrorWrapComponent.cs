using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class TerrorWrapComponent : Component
{
    /// <summary>
    /// DoAfter timer for cocooning someone.
    /// </summary>
    [DataField]
    public TimeSpan DoAfter = TimeSpan.FromSeconds(5);

    /// <summary>
    /// DoAfter timer for cocooning someone.
    /// </summary>
    [DataField]
    public EntProtoId CocoonProto = "TerrorCocoon";
}
