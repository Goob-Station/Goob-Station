// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Salvage.Magnet;

/// <summary>
/// Indicates the entity is a salvage target for tracking.
/// </summary>
[RegisterComponent]
public sealed partial class SalvageMagnetTargetComponent : Component
{
    /// <summary>
    /// Entity that spawned us.
    /// </summary>
    [DataField]
    public EntityUid DataTarget;
}
