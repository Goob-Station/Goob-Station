using Content.Goobstation.Shared.Disease.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Disease.Components;

[RegisterComponent]
public sealed partial class DiseaseSelectionComponent : Component
{
    [DataField]
    public EntProtoId? Disease;

    [DataField]
    public DiseaseSpreadSpecifier SpreadParams = new(1f, 1f, "Debug");

    [DataField]
    public int Min = 1;

    [DataField]
    public int Max = 2;
}
