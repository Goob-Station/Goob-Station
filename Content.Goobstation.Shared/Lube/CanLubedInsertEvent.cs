// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Lube;

/// <summary>
/// Raised on a lubed entity when there's an attempt to insert it into a container.
/// Set CanInsert to true to allow it to be inserted.
/// </summary>
[ByRefEvent]
public record struct CanLubedInsertEvent(ref readonly BaseContainer Into, bool CanInsert = false);
