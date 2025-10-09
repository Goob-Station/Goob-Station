using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CursedWeakComponent : Component
{
    /// <summary>
    /// How long before the stamina damage ticks and becomes stronger.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillIncrementStamina = TimeSpan.FromSeconds(15);

    /// <summary>
    /// How long before the drowsinness ticks and becomes stronger.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillIncrementDrowsy = TimeSpan.FromSeconds(55);

    /// <summary>
    /// How much stamina damage to apply over time.
    /// </summary>
    [DataField]
    public float StaminaDamageAmount;

    /// <summary>
    /// How much stamina damage to apply over time.
    /// </summary>
    [DataField]
    public float StaminaDamageIncrease = 25f;

    /// <summary>
    /// How much stamina damage can be received.
    /// </summary>
    [DataField]
    public float StaminaDamageMax = 200f;

    /// <summary>
    /// How much stamina damage to apply over time.
    /// </summary>
    [DataField]
    public TimeSpan SleepTimeAmount = TimeSpan.FromSeconds(0);

    /// <summary>
    /// How much stamina damage to apply over time.
    /// </summary>
    [DataField]
    public TimeSpan SleepTimeIncrease = TimeSpan.FromSeconds(1);

    /// <summary>
    /// How much stamina damage can be received.
    /// </summary>
    [DataField]
    public TimeSpan SleepTimeMax = TimeSpan.FromSeconds(8);

    /// <summary>
    /// Status effect to make you sleep.
    /// </summary>
    [DataField]
    public string ForcedSleep = "StatusEffectForcedSleeping";

    /// <summary>
    /// Next time at which stamina damage should increment.
    /// </summary>
    public TimeSpan NextTickStamina = TimeSpan.Zero;

    /// <summary>
    /// Next time at which drowsy should increment.
    /// </summary>
    public TimeSpan NextTickDrowsy = TimeSpan.Zero;
}
