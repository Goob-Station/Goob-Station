using Robust.Shared.GameStates;

namespace Content.Shared.Goobstation.Execution;

/// <summary>
/// Component that allow weapon to stun people for minute after doafter. Like Execution
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HitHeadComponent : Component
{
    /// <summary>
    /// Multiply duration of weapon cooldown
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DoAfterDurationMultiplier = 2f;

    /// <summary>
    /// Chance that head hit will apply sleep
    /// </summary>
    [DataField]
    public float SleepChance = 0.25f;

    /// <summary>
    /// Multiplier of hit damage
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DamageMultiplier = 2f;

    /// <summary>
    /// Sleep duration in seconds
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan SleepDuration = TimeSpan.FromSeconds(45);

    /// <summary>
    /// For multiplying damage only on doafter
    /// </summary>
    [DataField]
    public bool HitProccess = false;
}
