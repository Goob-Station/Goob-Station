using Content.Shared.Body.Part;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

// Quite the redundant name eh?
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryComponentConditionComponent : Component
{
    [DataField]
    public ComponentRegistry Component;

    [DataField]
    public bool Inverse;

}