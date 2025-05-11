// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.ProximityDetection.Components;

namespace Content.Shared.ProximityDetection;

/// <summary>
/// Raised to determine if proximity sensor can detect an entity.
/// </summary>
[ByRefEvent]
public struct ProximityDetectionAttemptEvent(float distance, Entity<ProximityDetectorComponent> detector, EntityUid target)
{
    public bool Cancelled;
    public readonly float Distance = distance;
    public readonly Entity<ProximityDetectorComponent> Detector = detector;
    public readonly EntityUid Target = target;
}

/// <summary>
/// Raised when distance from proximity sensor to the target was updated.
/// </summary>
[ByRefEvent]
public readonly record struct ProximityTargetUpdatedEvent(float Distance, Entity<ProximityDetectorComponent> Detector, EntityUid? Target = null);

/// <summary>
/// Raised when proximity sensor got new target.
/// </summary>
[ByRefEvent]
public readonly record struct NewProximityTargetEvent(float Distance, Entity<ProximityDetectorComponent> Detector, EntityUid? Target = null);
