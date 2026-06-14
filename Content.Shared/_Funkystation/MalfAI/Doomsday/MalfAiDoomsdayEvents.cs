// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Shared._Funkystation.MalfAI.Doomsday;

/// <summary>
/// Raised broadcast when a Malf AI initiates doomsday.
/// </summary>
[ByRefEvent]
public readonly record struct MalfAiDoomsdayStartedEvent(EntityUid Station, EntityUid Ai);

/// <summary>
/// Raised broadcast when the doomsday completes.
/// </summary>
[ByRefEvent]
public readonly record struct MalfAiDoomsdayCompletedEvent(EntityUid Station, EntityUid Ai);
