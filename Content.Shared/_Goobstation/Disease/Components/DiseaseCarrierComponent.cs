using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Disease;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DiseaseCarrierComponent : Component
{
    /// <summary>
    /// Currently contained diseases
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> Diseases = new();

    /// <summary>
    /// Diseases to add upon startup
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<EntProtoId>? StartingDiseases;
}
