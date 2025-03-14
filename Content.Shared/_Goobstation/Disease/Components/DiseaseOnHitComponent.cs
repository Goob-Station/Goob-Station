using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System;

namespace Content.Shared.Disease;

[RegisterComponent]
[EntityCategory("Diseases")]
public sealed partial class DiseaseOnHitComponent : Component
{
    /// <summary>
    /// Disease to give to entities hit with this item
    /// </summary>
    [DataField]
    public EntProtoId Disease;
}
