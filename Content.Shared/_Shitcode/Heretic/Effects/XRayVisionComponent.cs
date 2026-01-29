using Robust.Shared.GameStates;

namespace Content.Shared.Heretic.Effects;

[RegisterComponent, NetworkedComponent]
public sealed partial class XRayVisionComponent : Component
{
    [DataField]
    public bool EyeHadFov;
}
