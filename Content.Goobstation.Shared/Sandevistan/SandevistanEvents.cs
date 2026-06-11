// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Sandevistan;

public sealed partial class ToggleSandevistanEvent : InstantActionEvent;

/// <summary>
/// Raised to remove slowdown from an entity affected by a sandevistan slowfield.
/// </summary>
[ByRefEvent]
public record struct RemoveSandevistanSlowdownEvent(EntityUid Source);
