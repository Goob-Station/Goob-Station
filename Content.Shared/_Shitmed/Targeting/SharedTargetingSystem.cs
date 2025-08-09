// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kurokoTurbo <92106367+kurokoTurbo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Trest <144359854+trest100@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Kayzel <43700376+KayzelW@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Humanoid;

namespace Content.Shared._Shitmed.Targeting;
public abstract class SharedTargetingSystem : EntitySystem
{
    /// <summary>
    /// Returns all Valid target body parts as an array.
    /// </summary>
    public static TargetBodyPart[] GetValidParts()
    {
        var parts = new[]
        {
            TargetBodyPart.Head,
            TargetBodyPart.Chest,
            TargetBodyPart.Groin,
            TargetBodyPart.LeftArm,
            TargetBodyPart.LeftHand,
            TargetBodyPart.LeftLeg,
            TargetBodyPart.LeftFoot,
            TargetBodyPart.RightArm,
            TargetBodyPart.RightHand,
            TargetBodyPart.RightLeg,
            TargetBodyPart.RightFoot,
        };

        return parts;
    }

    public static HumanoidVisualLayers ToVisualLayers(TargetBodyPart targetBodyPart)
    {
        switch (targetBodyPart)
        {
            case TargetBodyPart.Head:
                return HumanoidVisualLayers.Head;
            case TargetBodyPart.Chest:
                return HumanoidVisualLayers.Chest;
            case TargetBodyPart.Groin:
                return HumanoidVisualLayers.Groin;
            case TargetBodyPart.LeftArm:
                return HumanoidVisualLayers.LArm;
            case TargetBodyPart.LeftHand:
                return HumanoidVisualLayers.LHand;
            case TargetBodyPart.RightArm:
                return HumanoidVisualLayers.RArm;
            case TargetBodyPart.RightHand:
                return HumanoidVisualLayers.RHand;
            case TargetBodyPart.LeftLeg:
                return HumanoidVisualLayers.LLeg;
            case TargetBodyPart.LeftFoot:
                return HumanoidVisualLayers.LFoot;
            case TargetBodyPart.RightLeg:
                return HumanoidVisualLayers.RLeg;
            case TargetBodyPart.RightFoot:
                return HumanoidVisualLayers.RFoot;
            default:
                return HumanoidVisualLayers.Chest;
        }
    }
}
