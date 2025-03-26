using Robust.Shared.GameStates;

namespace Content.Shared.Stunnable;

[RegisterComponent, NetworkedComponent]
public sealed partial class StamcritResistComponent : Component
{
    /// <summary>
    ///     If stamina damage reaches (damage * multiplier), then the entity will enter stamina crit.
    /// </summary>
    [DataField] public float Multiplier = 2f;
}
