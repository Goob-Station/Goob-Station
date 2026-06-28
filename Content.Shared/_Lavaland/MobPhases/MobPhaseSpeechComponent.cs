namespace Content.Shared._Lavaland.MobPhases;

[RegisterComponent]
public sealed partial class MobPhaseSpeechComponent : Component
{
    /// <summary>
    /// Speech definitions per phase.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<int, PhaseSpeechData> Phases = new();

    /// <summary>
    /// Minimum time between lines.
    /// </summary>
    [DataField]
    public float MinDelay = 10f;

    /// <summary>
    /// Maximum time between lines.
    /// </summary>
    [DataField]
    public float MaxDelay = 20f;

    /// <summary>
    /// The game time at which the entity is next allowed to speak a phase line.
    /// </summary>
    [ViewVariables]
    public TimeSpan NextSpeechTime;
}

[DataDefinition]
public sealed partial class PhaseSpeechData
{
    /// <summary>
    /// Lines that can be spoken during this phase, picked at random.
    /// </summary>
    [DataField]
    public List<LocId> Speech = new();

    /// <summary>
    /// Optional line spoken immediately upon entering this phase.
    /// </summary>
    [DataField]
    public LocId? SpeechOnPhaseChange;
}
