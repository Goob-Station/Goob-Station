// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.MalfunctionAi;

/// <summary>
/// Temporarily added to a machine that a Malfunction AI has set to overload.
/// After <see cref="TriggerAt"/> the machine explodes.
/// </summary>
[RegisterComponent]
public sealed partial class MalfPendingOverloadComponent : Component
{
    [DataField]
    public TimeSpan TriggerAt;

    [DataField]
    public float Intensity = 20f;

    [DataField]
    public float MaxTileIntensity = 5f;

    [DataField]
    public float Slope = 2f;

    [DataField]
    public EntityUid? Source;
}
