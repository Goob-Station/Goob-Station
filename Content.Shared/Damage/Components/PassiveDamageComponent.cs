using Content.Shared.Mobs;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.GameStates;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Passively damages the entity on a specified interval.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)] // Goobstation - Networked all  fields
public sealed partial class PassiveDamageComponent : Component
{
    /// <summary>
    /// The entitys' states that passive damage will apply in
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public List<MobState> AllowedStates = new();

    /// <summary>
    /// Damage / Healing per interval dealt to the entity every interval
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Delay between damage events in seconds
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Interval = 1f;

    /// <summary>
    /// The maximum HP the damage will be given to. If 0, disabled.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public FixedPoint2 DamageCap = 0;

    [DataField("nextDamage", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan NextDamage = TimeSpan.Zero;
}
