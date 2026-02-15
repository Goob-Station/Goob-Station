using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease.Components;

/// <summary>
/// Generates a random disease and grants it to the entity
/// </summary>
[RegisterComponent]
public sealed partial class GrantDiseaseComponent : Component
{
    /// <summary>
    /// Complexity of disease to grant
    /// </summary>
    [DataField]
    public float Complexity = 20f;

    /// <summary>
    /// The infection progress the disease starts out with
    /// </summary>
    public float Severity = 1f;

    /// <summary>
    /// Disease to use as a base to mutate from
    /// </summary>
    [DataField]
    public EntProtoId BaseDisease = "DiseaseBase";

    /// <summary>
    /// If not null, will set the disease to one of those types.
    /// </summary>
    [DataField]
    public List<ProtoId<DiseaseTypePrototype>>? PossibleTypes = null;
}
