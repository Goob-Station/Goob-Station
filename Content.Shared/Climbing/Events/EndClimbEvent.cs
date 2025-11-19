// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Climbing.Events;

/// <summary>
/// Raised on an entity when it ends climbing.
/// </summary>
[ByRefEvent]
public readonly record struct EndClimbEvent;
