using Content.Shared.Body.Organ;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryOrganConditionComponent : Component
{
    [DataField]
    public ComponentRegistry? Organ;

    [DataField]
    public bool Inverse;

    [DataField]
    public bool Reattaching;
}