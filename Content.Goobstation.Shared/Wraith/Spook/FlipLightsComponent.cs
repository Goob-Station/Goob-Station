using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Spook;

[RegisterComponent, NetworkedComponent]
public sealed partial class FlipLightsComponent : Component
{
    [DataField]
    public int FlipLightMaxTargets = 3;

    [DataField]
    public float FlipLightRadius = 1.5f;
}
