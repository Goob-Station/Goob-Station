// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Religion.Events;

[ByRefEvent]
public struct BeforeCastTouchSpellEvent(EntityUid target)
{
    /// <summary>
    /// The target of the event, to check if they meet the requirements for casting.
    /// </summary>
    public EntityUid? Target = target;

    /// <summary>
    /// Whether the event was cancelled.
    /// </summary>
    public bool Cancelled = false;
}
