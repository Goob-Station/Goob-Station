using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for Plane Shift ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingPlaneShiftComponent : Component
{
    [DataField]
    public bool IsActive;

    [DataField]
    public float WalkSpeedModifier = 1.25f;

    [DataField]
    public float RunSpeedModifier = 1.5f;

    [DataField]
    public EntProtoId ShadowWalkEffectOut = "ShadowlingShadowWalkOutEffect";
}
