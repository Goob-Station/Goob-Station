// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._NF.Storage.Components;

/// <summary>
/// Applies an ongoing pickup area around the attached entity.
/// </summary>
[RegisterComponent]
public sealed partial class MaterialStorageMagnetPickupComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("nextScan")]
    public TimeSpan NextScan = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadWrite), DataField("range")]
    public float Range = 0.3f;

    /// <summary>
    /// Frontier - Is the magnet currently enabled?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("magnetEnabled")]
    public bool MagnetEnabled = false;
}