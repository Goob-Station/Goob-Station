using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Throwing;

namespace Content.Shared._White.Xenomorphs.FaceHugger;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ThrowableFacehuggerComponent : Component
{
    /// <summary>
    /// The minimum force required for the facehugger to try to attach on impact.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MinThrowForce = 5f;

    /// <summary>
    /// Whether the facehugger is currently flying through the air.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool IsFlying = false;
}
