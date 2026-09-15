// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Oskarrr.Synthetic;

/// <summary>
/// Marks a humanoid synthetic: cable-coil repair target, no Biological medical heals.
/// </summary>
[RegisterComponent]
public sealed partial class SyntheticComponent : Component
{
    /// <summary>
    /// Brute healed per cable coil use.
    /// </summary>
    [DataField]
    public float CableHealAmount = 15f;

    /// <summary>
    /// DoAfter delay when repairing with cables (seconds).
    /// </summary>
    [DataField]
    public float CableDelay = 2f;
}
