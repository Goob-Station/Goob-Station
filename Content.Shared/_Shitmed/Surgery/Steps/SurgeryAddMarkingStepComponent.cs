using Content.Shared.Humanoid;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Steps;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryAddMarkingStepComponent : Component
{
    /// <summary>
    ///     The marking category to add the marking to.
    /// </summary>
    [DataField]
    public HumanoidVisualLayers MarkingCategory = default!;

    /// <summary>
    ///     Can be either a segment of a marking ID, or an entire ID that will be checked
    ///     against the entity to validate that the marking is not already present.
    /// </summary>
    [DataField]
    public String MatchString = "";

    /// <summary>
    ///     What type of organ is required for this surgery?
    /// </summary>
    [DataField]
    public ComponentRegistry? Organ;

    /// <summary>
    ///     Component name for accent that will be applied.
    /// </summary>
    [DataField]
    public ComponentRegistry? Accent;
}
