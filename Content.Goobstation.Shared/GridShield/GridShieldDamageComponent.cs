using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.GridShield;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class GridShieldDamageComponent : Component
{
    /// <summary>
    /// Last time the shield was damaged.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan LastTimeDamaged;

    /// <summary>
    /// Amount of seconds in which healing will be reduced after a successful hit.
    /// </summary>
    [DataField("healReduction")]
    public TimeSpan AfterHitHealReduction = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How much we reduce the healing after this shield was hit.
    /// </summary>
    [DataField]
    public float DebuffMultiplier = 0.5f;
}
