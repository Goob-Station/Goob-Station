// SPDX-License-Identifier: MIT

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Damage;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Passively damages the entity on a specified interval.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState] // Goobstation - Networked all  fields
public sealed partial class PassiveDamageComponent : Component
{
    /// <summary>
    /// The entitys' states that passive damage will apply in
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public List<MobState>? AllowedStates = null; // Goobstation

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

    /// <summary>
    /// Goobstation - How passive damage split damage between parts
    /// Split for damage and SplitEnsureAllDamagedAndOrganic for passive regen
    /// MOCHO, I DON'T CARE -> COME AND FIX YOUR MED!!
    /// </summary>
    [DataField]
    public SplitDamageBehavior SplitBehavior = SplitDamageBehavior.Split;
}
