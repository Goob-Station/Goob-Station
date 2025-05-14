// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Construction;

/// <summary>
/// Raised on the user after an entity is created by construction.
/// </summary>
[ByRefEvent]
public readonly record struct ConstructedEvent(EntityUid Entity);
