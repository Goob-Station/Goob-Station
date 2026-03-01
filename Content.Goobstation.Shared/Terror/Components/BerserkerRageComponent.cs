using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Component used to increase damage of a mob the lower its health is.
/// </summary>

[RegisterComponent, NetworkedComponent]
public sealed partial class BerserkerRageComponent : Component
{
    public float MinMultiplier = 1f;

    public float MaxMultiplier = 3f;

}
