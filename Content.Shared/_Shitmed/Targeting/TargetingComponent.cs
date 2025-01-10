using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Targeting;

/// <summary>
/// Controls entity limb targeting for actions.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TargetingComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TargetBodyPart Target = TargetBodyPart.Torso;

    /// <summary>
    /// What odds does the entity have of targeting each body part?
    /// </summary>
    [DataField]
    public Dictionary<TargetBodyPart, float> TargetOdds = new()
    {
        { TargetBodyPart.Head, 0.1f },
        { TargetBodyPart.Torso, 0.3f },
        { TargetBodyPart.Groin, 0.1f },
        { TargetBodyPart.LeftArm, 0.1f },
        { TargetBodyPart.LeftHand, 0.05f },
        { TargetBodyPart.RightArm, 0.1f },
        { TargetBodyPart.RightHand, 0.05f },
        { TargetBodyPart.LeftLeg, 0.1f },
        { TargetBodyPart.LeftFoot, 0.05f },
        { TargetBodyPart.RightLeg, 0.1f },
        { TargetBodyPart.RightFoot, 0.05f }
    };

    /// <summary>
    /// What is the current integrity of each body part?
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public Dictionary<TargetBodyPart, TargetIntegrity> BodyStatus = new()
    {
        { TargetBodyPart.Head, TargetIntegrity.Healthy },
        { TargetBodyPart.Torso, TargetIntegrity.Healthy },
        { TargetBodyPart.Groin, TargetIntegrity.Healthy },
        { TargetBodyPart.LeftArm, TargetIntegrity.Healthy },
        { TargetBodyPart.LeftHand, TargetIntegrity.Healthy },
        { TargetBodyPart.RightArm, TargetIntegrity.Healthy },
        { TargetBodyPart.RightHand, TargetIntegrity.Healthy },
        { TargetBodyPart.LeftLeg, TargetIntegrity.Healthy },
        { TargetBodyPart.LeftFoot, TargetIntegrity.Healthy },
        { TargetBodyPart.RightLeg, TargetIntegrity.Healthy },
        { TargetBodyPart.RightFoot, TargetIntegrity.Healthy }
    };

    /// <summary>
    /// What noise does the entity play when swapping targets?
    /// </summary>
    [DataField]
    public string SwapSound = "/Audio/Effects/toggleoncombat.ogg";
}
