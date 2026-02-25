using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.ShadowDemon.ShadowCocoon;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowCocoonComponent : Component
{
    /// <summary>
    ///  This should be the overall radius of the area the Shadow Cocoon occupies
    /// </summary>
    [DataField]
    public float Radius;
}
