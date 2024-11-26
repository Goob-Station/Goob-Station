using Robust.Shared.Prototypes;
using Content.Shared.Humanoid;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Steps;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryRemoveMarkingStepComponent : Component
{
    /// <summary>
    ///     The category the marking belongs to.
    /// </summary>
    [DataField]
    public HumanoidVisualLayers MarkingCategory = default!;

    /// <summary>
    ///     Can be either a segment of a marking ID, or an entire ID that will be checked
    ///     against the entity to validate that the marking is present.
    /// </summary>
    [DataField]
    public String MatchString = "";

    /// <summary>
    ///     Will this step spawn an item as a result of removing the markings? If so, which?
    /// </summary>
    [DataField]
    public EntProtoId? ItemSpawn = default!;

}
