// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Devil;

/// <summary>
/// Raised on a devil when their power level changes.
/// </summary>
/// <param name="User">The Devil whos power level is changing</param>
/// <param name="NewLevel">The new level they are reaching.</param>
[ByRefEvent]
public record struct PowerLevelChangedEvent(EntityUid User, int NewLevel);
