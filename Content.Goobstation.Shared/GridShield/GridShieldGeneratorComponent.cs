using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.GridShield;

/// <summary>
/// Generates a shield for a grid
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GridShieldGeneratorComponent : Component
{
    [DataField]
    public int Priority;

    /// <summary>
    /// Maximum health of that shield generator, represents how much damage it can
    /// handle before being down.
    /// </summary>
    [DataField]
    public float MaxDamage = 10000f;

    [DataField("damageEnt")]
    public EntProtoId DamageEntityId = "DamageGridShieldLow";

    /// <summary>
    /// Dummy entity to which all the damage gets redirected to.
    /// </summary>
    [DataField]
    public EntityUid DamageEntity;

    [ViewVariables]
    public bool Powered;
}
