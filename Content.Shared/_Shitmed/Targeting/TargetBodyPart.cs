// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Shitmed.Targeting;


/// <summary>
/// Represents and enum of possible target parts.
/// </summary>
/// <remarks>
/// To get all body parts as an Array, use static
/// method in SharedTargetingSystem GetValidParts.
/// </remarks>
[Flags]
public enum TargetBodyPart : ushort
{
    Head = 1,
    Torso = 1 << 1,
    Groin = 1 << 2,
    LeftArm = 1 << 3,
    LeftHand = 1 << 4,
    RightArm = 1 << 5,
    RightHand = 1 << 6,
    LeftLeg = 1 << 7,
    LeftFoot = 1 << 8,
    RightLeg = 1 << 9,
    RightFoot = 1 << 10,

    Hands = LeftHand | RightHand,
    Arms = LeftArm | RightArm,
    Legs = LeftLeg | RightLeg,
    Feet = LeftFoot | RightFoot,
    All = Head | Torso | Groin | LeftArm | LeftHand | RightArm | RightHand | LeftLeg | LeftFoot | RightLeg | RightFoot,
}