// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Present on an AI that bought the AI turret upgrade.
/// </summary>
[RegisterComponent]
public sealed partial class MalfTurretUpgradeComponent : Component
{
    /// <summary>Fire rate multiplier applied to AI turrets.</summary>
    [DataField] public float FireRateMultiplier = 2f;
}
