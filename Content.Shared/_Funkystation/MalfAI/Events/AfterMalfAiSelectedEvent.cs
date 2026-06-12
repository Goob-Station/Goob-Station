// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Raised after a Malf AI has been selected to set up their abilities.
/// </summary>
[ByRefEvent]
public record struct AfterMalfAiSelectedEvent(EntityUid EntityUid);
