// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Paper;

/// <summary>
/// 	Raised on the paper when a sign is successful.
/// </summary>
[ByRefEvent]
public record struct SignSuccessfulEvent(EntityUid Paper, EntityUid User);
