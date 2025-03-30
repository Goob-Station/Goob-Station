using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Server.Implants.Components;

[RegisterComponent]
public sealed partial class StypticStimulatorImplantComponent : Component
{
    /// <summary>
    /// The entitys' states that passive damage will apply in
    /// </summary>
    [DataField]
    public List<MobState> AllowedStates = [];

    /// <summary>
    /// Damage / Healing per interval dealt to the entity every interval
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Delay between damage events in seconds
    /// </summary>
    [DataField]
    public float Interval = 1f;

    /// <summary>
    /// The maximum HP the damage will be given to. If 0, disabled.
    /// </summary>
    [DataField]
    public FixedPoint2 DamageCap = 0;

    [DataField("nextDamage", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextDamage = TimeSpan.Zero;

}
