// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Map;

namespace Content.Server._Funkystation.MalfAI.Components;

/// <summary>
/// Added to the AI core for the duration of a gyroscope traversal.
/// Tracks interpolation state and entities already damaged on contact.
/// </summary>
[RegisterComponent]
public sealed partial class MalfGyroTraverseComponent : Component
{
    public MapCoordinates StartMap;
    public MapCoordinates EndMap;

    public Angle StartRotation;
    public Angle EndRotation;

    public TimeSpan StartTime;

    [DataField]
    public float DurationSeconds = 0.3f;

    /// <summary>
    /// Damage applied once per entity on contact during traversal.
    /// </summary>
    [DataField]
    public int ContactDamage = 50;

    /// <summary>
    /// Half extents for the traversal contact AABB (elongated along movement axis).
    /// </summary>
    [DataField]
    public float HalfExtentLong = 0.55f;

    [DataField]
    public float HalfExtentShort = 0.25f;

    /// <summary>
    /// Entities already damaged during this traversal, to avoid repeat hits.
    /// </summary>
    public HashSet<EntityUid> Damaged = new();
}
