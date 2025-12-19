using Content.Goobstation.Shared.Disease.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease.Components;

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

    /// <summary>
    /// Whether to be immune to disease effects
    /// For entities that need to carry disease but not have their effects happen
    /// </summary>
    [DataField]
    public bool EffectImmune = false;

    /// <summary>
    /// Icon to show on HUDs if total disease severity is low.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<DiseaseIconPrototype> LowIcon = "DiseaseIconLow";

    /// <summary>
    /// Icon to show on HUDs if total disease severity is medium.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<DiseaseIconPrototype> MediumIcon = "DiseaseIconMedium";

    /// <summary>
    /// Icon to show on HUDs if total disease severity is high.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<DiseaseIconPrototype> HighIcon = "DiseaseIconHigh";
}
