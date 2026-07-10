using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared._JustDecor.MartialArts.Components;

/// <summary>
/// Allows an NPC to be interrogated using the Big Boss CQC Interrogation combo.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NpcInterrogatableComponent : Component
{
    /// <summary>
    /// The ID of the interrogation dataset prototype.
    /// </summary>
    [DataField]
    public ProtoId<NpcInterrogationPrototype>? Prototype { get; set; }

    /// <summary>
    /// Has this NPC already been interrogated?
    /// </summary>
    [DataField]
    public bool Interrogated { get; set; }

    [ViewVariables]
    public List<string>? PendingLinkedAnswers { get; set; }
}

/// <summary>
/// Prototypical data for NPC interrogation responses.
/// </summary>
[Prototype("npcInterrogation")]
[Serializable, NetSerializable]
public sealed partial class NpcInterrogationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// List of general answers when interrogated.
    /// </summary>
    [DataField]
    public List<string> Answers { get; private set; } = new();

    /// <summary>
    /// List of general interrogation questions said by the interrogator.
    /// </summary>
    [DataField]
    public List<string> Questions { get; private set; } = new();

    /// <summary>
    /// Linked question/answer sets.
    /// </summary>
    [DataField]
    public List<NpcInterrogationLinkedEntry> Linked { get; private set; } = new();
}

/// <summary>
/// Component added to NPCs that have been "eliminated" (e.g. after interrogation).
/// This should prevent them from fighting back.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NpcEliminatedComponent : Component
{
    [DataField]
    public string? OriginalHTN;

    [DataField]
    public HashSet<string>? OriginalFactions;

    [DataField]
    public TimeSpan StartTime;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(15);
}

/// <summary>
/// Marks pacification that was applied specifically by Legendary CQC.
/// </summary>
[RegisterComponent]
public sealed partial class LegendaryCQCPacifiedComponent : Component
{
}

[DataDefinition, Serializable]
public sealed partial class NpcInterrogationLinkedEntry
{
    [DataField("question")]
    public string Question { get; set; } = string.Empty;

    [DataField("answers")]
    public List<string> Answers { get; set; } = new();
}
