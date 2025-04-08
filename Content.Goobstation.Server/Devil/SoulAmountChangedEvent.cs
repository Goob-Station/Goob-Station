// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Devil;

/// <summary>
/// Raised on a devil when the amount of souls in their storage changes.
/// </summary>
/// <param name="User">The Devil gaining souls.</param>
/// <param name="Victim">The entity losing its soul.</param>
/// <param name="Amount">How many souls they are gaining.</param>
[ByRefEvent]
public record struct SoulAmountChangedEvent(EntityUid User, EntityUid Victim, int Amount);
