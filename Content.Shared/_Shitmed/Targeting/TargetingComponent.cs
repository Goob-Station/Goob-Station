using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Targeting;

/// <summary>
/// Controls entity limb targeting for actions.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TargetingComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TargetBodyPart Target = TargetBodyPart.Chest;

    /// <summary>
    /// What odds are there for every part targeted to be hit?
    /// </summary>
    [DataField]
    public Dictionary<TargetBodyPart, Dictionary<TargetBodyPart, float>> TargetOdds = new()
    {
            {
            TargetBodyPart.Head, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.Head, 0.34f },
                { TargetBodyPart.Chest, 0.27f },
                { TargetBodyPart.LeftArm, 0.23f },
                { TargetBodyPart.RightArm, 0.23f },
            }
        },
        {
            TargetBodyPart.Chest, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.Chest, 0.34f },
                { TargetBodyPart.Groin, 0.34f },
                { TargetBodyPart.Head, 0.12f },
                { TargetBodyPart.RightArm, 0.24f },
                { TargetBodyPart.LeftArm, 0.24f },
            }
        },
        {
            TargetBodyPart.Groin, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.Groin, 0.34f },
                { TargetBodyPart.Chest, 0.34f },
                { TargetBodyPart.RightLeg, 0.17f },
                { TargetBodyPart.LeftLeg, 0.17f },
                { TargetBodyPart.RightArm, 0.11f },
                { TargetBodyPart.LeftArm, 0.11f },
            }
        },
        {
            TargetBodyPart.RightArm, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.RightArm, 0.45f },
                { TargetBodyPart.RightHand, 0.27f },
                { TargetBodyPart.Chest, 0.22f },
            }
        },
        {
            TargetBodyPart.LeftArm, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.LeftArm, 0.45f },
                { TargetBodyPart.LeftHand, 0.27f },
                { TargetBodyPart.Chest, 0.22f },
            }
        },
        {
            TargetBodyPart.RightHand, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.RightHand, 0.75f },
                { TargetBodyPart.RightArm, 0.25f },
            }
        },
        {
            TargetBodyPart.LeftHand, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.LeftHand, 0.75f },
                { TargetBodyPart.LeftArm, 0.25f },
            }
        },
        {
            TargetBodyPart.RightLeg, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.RightLeg, 0.67f },
                { TargetBodyPart.RightFoot, 0.23f },
                { TargetBodyPart.Groin, 0.1f },
            }
        },
        {
            TargetBodyPart.LeftLeg, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.LeftLeg, 0.67f },
                { TargetBodyPart.LeftFoot, 0.23f },
                { TargetBodyPart.Groin, 0.1f },
            }
        },
        {
            TargetBodyPart.RightFoot, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.RightFoot, 0.75f },
                { TargetBodyPart.RightLeg, 0.25f },
            }
        },
        {
            TargetBodyPart.LeftFoot, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.LeftFoot, 0.75f },
                { TargetBodyPart.LeftLeg, 0.25f },
            }
        },
    };

    /// <summary>
    /// What is the current integrity of each body part?
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public Dictionary<TargetBodyPart, WoundableSeverity> BodyStatus = new()
    {
        { TargetBodyPart.Head, WoundableSeverity.Healthy },
        { TargetBodyPart.Chest, WoundableSeverity.Healthy },
        { TargetBodyPart.Groin, WoundableSeverity.Healthy },
        { TargetBodyPart.LeftArm, WoundableSeverity.Healthy },
        { TargetBodyPart.LeftHand, WoundableSeverity.Healthy },
        { TargetBodyPart.RightArm, WoundableSeverity.Healthy },
        { TargetBodyPart.RightHand, WoundableSeverity.Healthy },
        { TargetBodyPart.LeftLeg, WoundableSeverity.Healthy },
        { TargetBodyPart.LeftFoot, WoundableSeverity.Healthy },
        { TargetBodyPart.RightLeg, WoundableSeverity.Healthy },
        { TargetBodyPart.RightFoot, WoundableSeverity.Healthy },
    };

    /// <summary>
    /// What noise does the entity play when swapping targets?
    /// </summary>
    [DataField]
    public string SwapSound = "/Audio/Effects/toggleoncombat.ogg";
}
