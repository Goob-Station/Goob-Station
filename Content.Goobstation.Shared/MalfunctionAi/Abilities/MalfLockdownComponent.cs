// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Tuning and live state for the Hostile Station Lockdown ability.
/// </summary>
[RegisterComponent]
public sealed partial class MalfLockdownComponent : Component
{
    /// <summary>How long a station lockdown keeps doors bolted and electrified, in seconds.</summary>
    [DataField] public float Duration = 90f;

    /// <summary>When the current lockdown ends. Null if no lockdown is active.</summary>
    [DataField] public TimeSpan? EndTime;

    /// <summary>Doors affected by the current lockdown, to be reverted when it ends.</summary>
    [DataField] public List<EntityUid> LockedDoors = new();
}
