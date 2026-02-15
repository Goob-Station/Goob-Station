using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease.Components;

[RegisterComponent]
public sealed partial class DiseaseGrantComponentEffectComponent : Component
{
    [DataField(required: true)]
    public ComponentRegistry Components;

    [DataField]
    public bool RemoveOnCure = true;
}
