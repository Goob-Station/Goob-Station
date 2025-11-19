// SPDX-License-Identifier: MIT

namespace Content.Server._Funkystation.Factory.Components;

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
