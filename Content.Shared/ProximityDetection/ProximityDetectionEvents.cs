// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;
using Content.Shared.ProximityDetection.Components;

namespace Content.Shared.ProximityDetection;

[ByRefEvent]
public record struct ProximityDetectionAttemptEvent(bool Cancel, FixedPoint2 Distance, Entity<ProximityDetectorComponent> Detector);

[ByRefEvent]
public record struct ProximityTargetUpdatedEvent(ProximityDetectorComponent Detector, EntityUid? Target, FixedPoint2 Distance);

[ByRefEvent]
public record struct NewProximityTargetEvent(ProximityDetectorComponent Detector, EntityUid? Target);


