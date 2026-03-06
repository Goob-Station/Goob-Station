using Robust.Shared.Localization;

namespace Content.Shared._Lavaland.Megafauna.Components;

[RegisterComponent]
public sealed partial class MegafaunaPhaseSpeechComponent : Component
{
    /// <summary>
    /// Speech definitions per phase.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<int, PhaseSpeechData> Phases = new();

    /// <summary>
    /// Minimum time between ambient speech lines.
    /// </summary>
    [DataField]
    public float MinDelay = 10f;

    /// <summary>
    /// Maximum time between ambient speech lines.
    /// </summary>
    [DataField]
    public float MaxDelay = 20f;

    // Runtime state (not serialized)
    [ViewVariables]
    public float NextSpeechTime;
}

[DataDefinition]
public sealed partial class PhaseSpeechData
{
    /// <summary>
    /// Lines that can be spoken during this phase.
    /// Chosen randomly when requested.
    /// </summary>
    [DataField]
    public List<LocId> Speech = new();

    /// <summary>
    /// Optional line spoken immediately upon entering this phase.
    /// </summary>
    [DataField]
    public LocId? SpeechOnPhaseChange;
}
