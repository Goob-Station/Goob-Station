using Robust.Shared.GameStates;

[RegisterComponent, NetworkedComponent]
public sealed partial class CursedWeakComponent : Component
{
    /// <summary>
    /// How long before the blindness curse ticks and becomes stronger.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillIncrement = TimeSpan.FromSeconds(15);

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
    public TimeSpan SleepTimeMax = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Status effect to make you sleep.
    /// </summary>
    [DataField]
    public string StatusEffectKey = "ForcedSleep";

    /// <summary>
    /// Needed for death curse later. Gets set to true once max stacks are in.
    /// </summary>
    [DataField]
    public bool WeakCurseFullBloom;

    /// <summary>
    /// Next time at which blindness should increment.
    /// </summary>
    public TimeSpan NextTick = TimeSpan.Zero;
}
