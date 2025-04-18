// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Religion.Events;

[ByRefEvent]
public struct BeforeCastTouchSpellEvent(EntityUid performer, EntityUid? target)
{
    /// <summary>
    /// The target of the event, to check if they meet the requirements for casting.
    /// </summary>
    public EntityUid Performer = performer;

    /// <summary>
    /// The target of the event, to check if they meet the requirements for casting.
    /// </summary>
    public EntityUid? Target = target;

    public bool Cancelled = false;
}
