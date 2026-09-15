using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Sandevistan;

/// <summary>
/// Applied to entities affected by a sandevistan slowfield.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SandevistanSlowedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Source;

    [DataField, AutoNetworkedField]
    public float SpeedMultiplier = 1f;

    [DataField, AutoNetworkedField]
    public Vector2 OriginalLinearVelocity;

    /// <summary>
    /// Whether this entity is currently actively slowed.
    /// False means the slowdown was removed but the component is pending cleanup.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsSlowed = true;

    [DataField, AutoNetworkedField]
    public EntityUid? SlowedWeapon;

    [DataField, AutoNetworkedField]
    public float SlowedWeaponOriginalAttackRate;

    /// <summary>
    /// Whether this entity is currently flying through the air from a sandevistan trample.
    /// </summary>
    [DataField]
    public bool Trampled;
}
