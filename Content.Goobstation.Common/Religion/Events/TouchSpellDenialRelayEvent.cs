// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Religion.Events;

/// <summary>
/// Event broadcast when a touch spell is cancelled.
/// </summary>
[ByRefEvent]
public record struct TouchSpellDenialRelayEvent(bool Cancelled = false);
