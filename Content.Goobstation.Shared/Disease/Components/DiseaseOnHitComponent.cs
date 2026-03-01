using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease.Components;

[RegisterComponent]
public sealed partial class DiseaseOnHitComponent : Component
{
    /// <summary>
    /// Disease to give to entities hit with this
    /// If null, will spread diseases had by this entity
    /// </summary>
    [DataField]
    public EntProtoId? Disease;

    [DataField]
    public DiseaseSpreadSpecifier SpreadParams = new(1f, 1f, "Debug");
}
