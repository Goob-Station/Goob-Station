using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Common.Stunnable;

[RegisterComponent, NetworkedComponent]
public sealed partial class StamcritResistComponent : Component
{
    /// <summary>
    ///     If stamina damage reaches (damage * multiplier), then the entity will enter stamina crit.
    /// </summary>
    [DataField] public float Multiplier = 2f;
}
