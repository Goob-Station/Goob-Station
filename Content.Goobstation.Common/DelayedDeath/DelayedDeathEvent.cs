// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.DelayedDeath;

/// <summary>
/// 	Raised on a user when delayed death is triggered on them.
///     (E.G, they die to it.)
/// </summary>
[ByRefEvent]
public record struct DelayedDeathEvent(EntityUid User, bool Cancelled = false);
