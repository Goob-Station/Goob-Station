using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Stunnable;

[RegisterComponent, NetworkedComponent]
public sealed partial class StaminaDamageResistanceComponent : Component
{
    /// <summary>
    ///     1 - no reduction, 0 - full reduction
    /// </summary>
    [DataField] public float Coefficient = 1;
} 
