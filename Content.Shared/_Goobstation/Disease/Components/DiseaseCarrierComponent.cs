using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Disease;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedDiseaseSystem))] // add/remove diseases using the system's methods
public sealed partial class DiseaseCarrierComponent : Component
{
    /// <summary>
    /// Currently contained diseases
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> Diseases = new();

    /// <summary>
    /// Diseases to add on component startup
    /// </summary>
    [DataField("diseases")]
    public List<EntProtoId> StartingDiseases = new();
}
