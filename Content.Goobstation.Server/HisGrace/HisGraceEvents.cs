// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.HisGrace;

namespace Content.Goobstation.Server.HisGrace;

/// <summary>
/// Raised on His Grace when the hunger level changes.
/// </summary>
/// <param name="NewState">The new hunger level of His Grace.</param>
[ByRefEvent]
public record struct HisGraceStateChangedEvent(HisGraceState NewState, HisGraceState OldState);

[ByRefEvent]
public record struct HisGraceHungerChangedEvent;

/// <summary>
/// Raised on His Grace when an entity is consumed
/// </summary>
[ByRefEvent]
public record struct HisGraceEntityConsumedEvent;
