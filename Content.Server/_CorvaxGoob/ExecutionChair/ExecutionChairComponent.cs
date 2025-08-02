using Content.Shared.DeviceLinking;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Server._CorvaxNext.ExecutionChair;

namespace Content.Server._CorvaxNext.ExecutionChair;

/// <summary>
/// This component represents the state and configuration of an Execution Chair entity.
/// It holds data fields that determine how the chair behaves when it delivers electric shocks
/// to entities buckled into it. It also provides fields for connecting to and receiving signals
/// from the device linking system.
/// </summary>
[RegisterComponent, Access(typeof(ExecutionChairSystem))]
public sealed partial class ExecutionChairComponent : Component
{
    /// <summary>
    /// The next scheduled time at which this chair can deliver damage to strapped entities.
    /// This is used to control the rate of repeated electrocution ticks.
    /// </summary>
    [ViewVariables]
    public TimeSpan NextDamageTick = TimeSpan.Zero;

    /// <summary>
    /// Indicates whether the chair is currently enabled. If true, and all conditions (powered, anchored, etc.)
    /// are met, the chair will deliver electrical damage to any buckled entities at regular intervals.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = false;

    /// <summary>
    /// Determines whether the chair should play a sound when entities are shocked. If set to true,
    /// a sound from <see cref="ShockNoises"/> will be played each time damage is dealt.
    /// </summary>
    [DataField]
    public bool PlaySoundOnShock = true;

    /// <summary>
    /// Specifies which sound collection is played when entities are shocked. By default, uses a collection of
    /// "sparks" sounds. This allows multiple random sparks audio clips to be played.
    /// </summary>
    [DataField]
    public SoundSpecifier ShockNoises = new SoundCollectionSpecifier("sparks");

    /// <summary>
    /// Controls how loud the shock sound is. This value is applied to the base volume of the chosen sound
    /// when played.
    /// </summary>
    [DataField]
    public float ShockVolume = 20;

    /// <summary>
    /// The amount of damage delivered to a buckled entity each damage tick while the chair is active.
    /// </summary>
    [DataField]
    public int DamagePerTick = 25;

    /// <summary>
    /// The duration in seconds for which the electrocution effect is applied each time damage is dealt.
    /// For example, if set to 4, it electrocutes an entity for 4 seconds.
    /// </summary>
    [DataField]
    public int DamageTime = 4;

    /// <summary>
    /// The name of the device link port used to toggle the chair's state. Receiving a signal on this port
    /// switches the enabled state from on to off or from off to on.
    /// </summary>
    [DataField]
    public string TogglePort = "Toggle";

    /// <summary>
    /// The name of the device link port used to force the chair's state to enabled (on).
    /// Receiving a signal here ensures the chair is active.
    /// </summary>
    [DataField]
    public string OnPort = "On";

    /// <summary>
    /// The name of the device link port used to force the chair's state to disabled (off).
    /// Receiving a signal here ensures the chair is inactive.
    /// </summary>
    [DataField]
    public string OffPort = "Off";
}
