// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.Map;

namespace Content.Server._Funkystation.MalfAI;

// Server-only component used to time a gyroscope traversal.
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

    // Half extents for the traversal contact AABB (elongated along movement axis).
    [DataField]
    public float HalfExtentLong = 0.55f;

    [DataField]
    public float HalfExtentShort = 0.25f;

    [DataField]
    public DamageSpecifier ContactDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 50 }
        }
    };

    // Track entities already damaged during this traversal to avoid repeat hits.
    public HashSet<EntityUid> Damaged = new();
}
