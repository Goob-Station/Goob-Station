// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Religion;

public sealed class TouchSpellDenialRelayEvent : CancellableEntityEventArgs;

public sealed class BeforeCastTouchSpellEvent(EntityUid target) : CancellableEntityEventArgs
{
    /// <summary>
    /// The target of the event, to check if they meet the requirements for casting.
    /// </summary>
    public EntityUid? Target = target;
}
