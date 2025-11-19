// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Shuttles.Events;

/// <summary>
/// Raised broadcast whenever a shuttle FTLs
/// </summary>
[ByRefEvent]
public readonly record struct ShuttleFlattenEvent(EntityUid MapUid, List<Box2> AABBs);
