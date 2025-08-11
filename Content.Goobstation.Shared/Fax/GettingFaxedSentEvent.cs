// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Fax;
using Content.Shared.Fax.Components;

namespace Content.Goobstation.Shared.Fax;

/// <summary>
/// Raised on an entity when it's getting sent with a fax.
/// Set Handled to true to cancel normal fax behavior.
/// </summary>
[ByRefEvent]
public record struct GettingFaxedSentEvent(ref readonly Entity<FaxMachineComponent> Fax, ref readonly FaxSendMessage Args, bool Handled = false);
