// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Rainfey <rainfey0+github@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared.Roles;

/// <summary>
///     Event raised on a mind entity id to get whether or not the player is considered an antagonist,
///     depending on their roles.
/// </summary>
/// <param name="IsAntagonist">Whether or not the player is an antagonist.</param>
/// <param name="IsExclusiveAntagonist">Whether or not AntagSelectionSystem should exclude this player from other antag roles</param
[ByRefEvent]
public record struct MindIsAntagonistEvent(bool IsAntagonist, bool IsExclusiveAntagonist);