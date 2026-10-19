// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace Content.Server.MalfAI;

// Server-only component used to time a gyroscope traversal.
[RegisterComponent]
public sealed partial class MalfGyroTraverseComponent : Component
{
    public MapCoordinates StartMap;
    public MapCoordinates EndMap;

    public Angle StartRotation;
    public Angle EndRotation;

    public TimeSpan StartTime;

    [DataField] public float DurationSeconds = 0.3f;

    // Damage applied once per entity on contact during traversal.
    [DataField] public int ContactDamage = 50;

    // Half extents for the traversal contact AABB (elongated along movement axis).
    [DataField] public float HalfExtentLong = 0.55f;

    [DataField] public float HalfExtentShort = 0.25f;

    // Track entities already damaged during this traversal to avoid repeat hits.
    public HashSet<EntityUid> Damaged = new();
}
