using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DecayComponent : Component
{
    /// <summary>
    /// How much stamina damage to apply over time.
    /// </summary>
    [DataField]
    public float StaminaDamageAmount = 150f;
}
