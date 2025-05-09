// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goidastation.Common.Paper;

/// <summary>
/// 	Raised on the paper when a sign is attempted
/// </summary>
[ByRefEvent]
public record struct BeingSignedAttemptEvent(EntityUid Paper, EntityUid Signer, bool Cancelled = false);
