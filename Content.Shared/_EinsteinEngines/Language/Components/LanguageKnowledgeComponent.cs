using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._EinsteinEngines.Language.Components;

/// <summary>
/// Goobstation edit
/// Assigned to the knowledge entity that holds information about what languages the parent knows.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LanguageKnowledgeComponent : Component
{
    /// <summary>
    ///     List of languages this entity can speak without any external tools.
    /// </summary>
    [DataField]
    public List<ProtoId<LanguagePrototype>> SpokenLanguages = new();

    /// <summary>
    ///     List of languages this entity can understand without any external tools.
    /// </summary>
    [DataField]
    public List<ProtoId<LanguagePrototype>> UnderstoodLanguages = new();
}
