using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Default slasher fear overlay.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlasherFearOverlayComponent : Component
{
    [DataField, AutoNetworkedField]
    public float FadeSpeed = 1.5f;

    [DataField, AutoNetworkedField]
    public float BaseIntensity = 0.15f;
}
