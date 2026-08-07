using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Projectiles;

/// <summary>
/// Passively dodges incoming projectiles and hitscan shots instead of being hit by them,
/// playing a dodge animation on the entity's sprite.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class AutoDodgeComponent : Component
{
    /// <summary>
    /// Sound played every time a shot is evaded.
    /// </summary>
    [DataField]
    public SoundSpecifier? DodgeSound =
        new SoundCollectionSpecifier("BulletMiss", AudioParams.Default.WithVariation(0.125f));

    /// <summary>
    /// Animation key the dodge animation plays under.
    /// </summary>
    [DataField]
    public string AnimationKey = "auto-dodge";

    [DataField]
    public float SidestepDistance = 0.3f;

    [DataField]
    public float LeanDegrees = 28f;

    /// <summary>
    /// How long the dodge animation takes.
    /// </summary>
    [DataField]
    public TimeSpan AnimationLength = TimeSpan.FromSeconds(0.35);

    /// <summary>
    /// Minimum time between dodge effects.
    /// </summary>
    [DataField]
    public TimeSpan EffectCooldown = TimeSpan.FromSeconds(0.05);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextEffectTime;
}
