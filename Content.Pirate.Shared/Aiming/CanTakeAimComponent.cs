using System.Collections.Generic;
using Content.Shared.Damage;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Pirate.Shared.Aiming;

/// <summary>
/// Allows the weapon entity to be used to aim at other entities.
/// </summary>
[RegisterComponent]
public sealed partial class CanTakeAimComponent : Component
{
    public bool IsAiming = false;
    public EntityUid? User = null;
    public double AimStartFrame = 0;
    public List<EntityUid> AimingAt = new();
    /// <summary>
    /// How long does it take for your shot to fully "charge" in seconds
    /// </summary>
    [DataField] public float MaxAimTime = 5f;
    /// <summary>
    /// Maximal damage boost you can get from aiming
    /// </summary>
    [DataField] public short MaxDamageMultiplier = 2;
}
