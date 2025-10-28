// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com> Goobstation
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

// This all Goobstation - used for sprinting event
namespace Content.Shared.Damage.Events;

/// <summary>
/// Goobstaiton - Raised when an entity's sprinting state changes.
/// </summary>
[ByRefEvent]
public readonly record struct SprintingStateChangedEvent(EntityUid Uid, bool IsSprinting);
