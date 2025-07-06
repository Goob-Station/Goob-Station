using Robust.Shared.GameStates;

namespace Content.Shared._DV.Weapons.Ranged.Components;

/// <summary>
/// Alters the accuracy of attached entity's held or wielded guns via
/// <see cref="Shared.Weapons.Ranged.Events.GunRefreshModifiersEvent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PlayerAccuracyModifierComponent : Component
{
    /// <summary>
    /// Multiplies the Min/Max angles of a gun by this amount.
    /// </summary>
    [DataField]
    public float SpreadMultiplier = 15f;

    /// <summary>
    /// Maximum angle, in degrees, an entity can shoot between.
    /// After the SpreadMultiplier is applied, this clamp can stop the entity
    /// from shooting behind themselves.
    /// </summary>
    [DataField]
    public float MaxSpreadAngle = 180f;
}
