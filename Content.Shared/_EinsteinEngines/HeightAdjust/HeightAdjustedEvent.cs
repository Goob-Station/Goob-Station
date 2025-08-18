// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 MarkerWicker <markerWicker@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;

namespace Content.Shared._EinsteinEngines.HeightAdjust;

/// <summary>
///     Raised on a humanoid after their scale has been adjusted in accordance with their profile and their physics have been updated.
/// </summary>
public sealed class HeightAdjustedEvent : EntityEventArgs
{
    public Vector2 NewScale;
}
