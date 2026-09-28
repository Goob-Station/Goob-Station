using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease.Components;

[RegisterComponent]
public sealed partial class DiseaseSelectionComponent : Component
{
    [DataField]
    public EntProtoId? Disease;

    [DataField]
    public DiseaseSpreadSpecifier SpreadParams = new(1f, 1f, "Debug");

    [DataField, AutoNetworkedField]
    public float Min = 1f;

    [DataField, AutoNetworkedField]
    public float Max = 2f;
}
