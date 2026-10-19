// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Marks an entity to be omitted from camera upgrade considerations.
/// This component prevents entities from being treated as cameras for MalfAI camera upgrade functionality.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CameraUpgradeOmitterComponent : Component
{
}
