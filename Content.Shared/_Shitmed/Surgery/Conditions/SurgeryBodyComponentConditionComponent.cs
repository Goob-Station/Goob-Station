using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

// <summary>
//   What components are necessary in the body for the surgery to be valid.
// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryBodyComponentConditionComponent : Component
{
    // <summary>
    //   The components to check for.
    // </summary>
    [DataField(required: true)]
    public ComponentRegistry Components;

    // <summary>
    //   If true, the lack of these components will instead make the surgery valid.
    // </summary>
    [DataField]
    public bool Inverse = false;
}
