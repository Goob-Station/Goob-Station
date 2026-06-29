using Robust.Shared.GameStates;

namespace Content.Shared.Overlays;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SaturationScaleOverlayComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SaturationScale = 1f;

    [DataField, AutoNetworkedField]
    public float FadeInMultiplier = 0.1f;
}
