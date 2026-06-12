// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Server._Funkystation.MalfAI.Factory;

/// <summary>
/// Server-side marker that stores which Malf AI built a RoboticsFactoryGrid.
/// Used by CyborgFactorySystem to assign created borgs to the correct AI controller.
/// </summary>
[RegisterComponent]
public sealed partial class MalfFactoryOwnerComponent : Component
{
    /// <summary>
    /// The Malf AI entity that requested/built this factory.
    /// </summary>
    [DataField]
    public EntityUid? Controller;
}
