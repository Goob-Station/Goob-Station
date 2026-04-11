using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Bloodsuckers.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class BloodsuckerDayNightComponent : Component
{
    /// <summary>
    /// Is it currently daytime?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsDaytime;

    /// <summary>
    /// Seconds remaining until the next cycle change.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float TimeUntilCycle = 720f;

    // Night duration
    [DataField]
    public float NightDurationMin = 540f; // 720 - 180 delay
    [DataField]
    public float NightDurationMax = 900f; // 720 + 180 delay

    // Day duration
    [DataField]
    public float DayDurationMin = 40f;
    [DataField]
    public float DayDurationMax = 65f;

    // Warning thresholds (seconds before day starts)
    [DataField]
    public float WarnFirst = 90f;
    [DataField]
    public float WarnFinal = 30f;
    [DataField]
    public float WarnImminent = 5f;

    /// <summary>
    /// Whether the first warning has already been sent this cycle.
    /// </summary>
    [DataField]
    public bool SentFirstWarning;
    [DataField]
    public bool SentFinalWarning;
    [DataField]
    public bool SentImminentWarning;

    [DataField]
    public SoundSpecifier? DayStartSound = new SoundPathSpecifier("/Audio/_Goobstation/Bloodsucker/Effects/sol_rise.ogg");
    [DataField]
    public SoundSpecifier? DayEndSound = new SoundPathSpecifier("/Audio/_Goobstation/Bloodsucker/Effects/sol_end.ogg");
}
