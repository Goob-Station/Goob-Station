using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Steps;

[RegisterComponent, NetworkedComponent]
[Prototype("SurgerySteps")]
public sealed partial class SurgeryStepComponent : Component
{

    [DataField]
    public ComponentRegistry? Tool;

    [DataField]
    public ComponentRegistry? Add;

    [DataField]
    public ComponentRegistry? BodyAdd;

    [DataField]
    public ComponentRegistry? Remove;

    [DataField]
    public ComponentRegistry? BodyRemove;

    /// <summary>
    ///   These components will be added to the body part's organs' OnAdd field.
    ///   Each key is the SlotId of the organ to look for.
    ///
    ///   Used to make organs add components to whatever body it's residing in.
    /// </summary>
    [DataField]
    public Dictionary<string, ComponentRegistry>? AddOrganOnAdd;

    /// <summary>
    ///   These components will be removed from the body part's organs' OnAdd field.
    ///   Each key is the SlotId of the organ to look for.
    ///
    ///   Used to stop organs from adding components to whatever body it's residing in.
    /// </summary>
    [DataField]
    public Dictionary<string, ComponentRegistry>? RemoveOrganOnAdd;

    [DataField]
    public float Duration = 2f;
}
