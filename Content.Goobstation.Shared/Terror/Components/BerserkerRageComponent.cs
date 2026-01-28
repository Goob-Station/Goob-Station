using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BerserkerRageComponent : Component
{
    public float MinMultiplier = 1f;

    public float MaxMultiplier = 3f;

}
