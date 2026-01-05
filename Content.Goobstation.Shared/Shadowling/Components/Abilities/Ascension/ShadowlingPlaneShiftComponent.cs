using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;

/// <summary>
/// This is used for Plane Shift ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingPlaneShiftComponent : Component
{
    /// <summary>
    /// Indicates whether the ability is active, or not.
    /// </summary>
    [DataField]
    public bool IsActive;

    /// <summary>
    /// Indicates the new walking speed of the ability user.
    /// </summary>
    [DataField]
    public float WalkSpeedModifier = 1.25f;

    /// <summary>
    /// Indicates the new running speed of the ability user.
    /// </summary>
    [DataField]
    public float RunSpeedModifier = 1.5f;

    [DataField]
    public EntProtoId ActionId = "ActionPlaneShift";

    [DataField]
    public EntityUid? ActionEnt;
}
