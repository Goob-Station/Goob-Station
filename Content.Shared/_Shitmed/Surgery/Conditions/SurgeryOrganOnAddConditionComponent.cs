using Content.Shared.Body.Part;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

// <summary>
//   What components are necessary in the part's organs' OnAdd fields for the surgery to be valid.
//
//   Not all components need to be present (or missing for Inverse = true). At least one component matching (or missing) can make the surgery valid.
// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryOrganOnAddConditionComponent : Component
{
    // <summary>
    //   The components to check for on each organ, with the key being the organ's SlotId.
    // </summary>
    [DataField(required: true)]
    public Dictionary<string, ComponentRegistry> Components;

    // <summary>
    //   If true, the lack of these components will instead make the surgery valid.
    // </summary>
    [DataField]
    public bool Inverse = false;
}