// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
    /// Goobstation
    /// Chance to miss and hit torso instead of the targeted part on heavy attacks
    /// Applies to prone targets as well
    /// </summary>
    [DataField]
    public float HeavyAttackMissChance = 0.5f;

    /// <summary>
    /// What odds are there for every part targeted to be hit?
    /// </summary>
    [DataField]
    public Dictionary<TargetBodyPart, Dictionary<TargetBodyPart, float>> TargetOdds = new()
    {
        {
            TargetBodyPart.Head, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.Head, 0.5f },
                { TargetBodyPart.Chest, 0.5f },
            }
        },
        {
            TargetBodyPart.Chest, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.Chest, 1f }, // If you change this, suicide system won't work properly. So I won't even be able to ask you to kill yourself for doing this.
            }
        },
        {
            TargetBodyPart.Groin, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.Groin, 0.2f },
                { TargetBodyPart.Chest, 0.8f },
            }
        },
        {
            TargetBodyPart.RightArm, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.RightArm, 0.7f },
                { TargetBodyPart.Chest, 0.2f },
                { TargetBodyPart.RightHand, 0.1f },
            }
        },
        {
            TargetBodyPart.LeftArm, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.LeftArm, 0.7f },
                { TargetBodyPart.Chest, 0.2f },
                { TargetBodyPart.LeftHand, 0.1f },
            }
        },
        {
            TargetBodyPart.RightHand, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.RightHand, 0.3f },
                { TargetBodyPart.Chest, 0.4f },
                { TargetBodyPart.RightArm, 0.3f },
            }
        },
        {
            TargetBodyPart.LeftHand, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.LeftHand, 0.3f },
                { TargetBodyPart.Chest, 0.4f },
                { TargetBodyPart.LeftArm, 0.3f },
            }
        },
        {
            TargetBodyPart.RightLeg, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.RightLeg, 0.7f },
                { TargetBodyPart.Chest, 0.2f },
                { TargetBodyPart.RightFoot, 0.1f },
            }
        },
        {
            TargetBodyPart.LeftLeg, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.LeftLeg, 0.7f },
                { TargetBodyPart.Chest, 0.2f },
                { TargetBodyPart.LeftFoot, 0.1f },
            }
        },
        {
            TargetBodyPart.RightFoot, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.RightFoot, 0.3f },
                { TargetBodyPart.Chest, 0.4f },
                { TargetBodyPart.RightLeg, 0.3f },
            }
        },
        {
            TargetBodyPart.LeftFoot, new Dictionary<TargetBodyPart, float>
            {
                { TargetBodyPart.LeftFoot, 0.3f },
                { TargetBodyPart.Chest, 0.4f },
                { TargetBodyPart.LeftLeg, 0.3f },
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
