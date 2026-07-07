// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Placed on an AI that bought the camera network upgrade: its cameras see through walls and further
/// and become EMP-proof, and the AI gains a toggleable night-vision overlay.
/// </summary>
[RegisterComponent]
public sealed partial class MalfCameraUpgradeComponent : Component
{
    /// <summary>Camera vision range after the upgrade, through walls included (vanilla range is 7.5).</summary>
    [DataField] public float Range = 12f;
}
