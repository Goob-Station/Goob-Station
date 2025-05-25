// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Centronias <charlie.t.santos@gmail.com>
// SPDX-FileCopyrightText: 2025 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Wieldable;

/// <summary>
/// Raised directed on an item when it is wielded.
/// </summary>
[ByRefEvent]
public readonly record struct ItemWieldedEvent(EntityUid User);

/// <summary>
/// Raised directed on an item that has been unwielded.
/// Force is whether the item is being forced to be unwielded, or if the player chose to unwield it themselves.
/// </summary>
[ByRefEvent]
public readonly record struct ItemUnwieldedEvent(EntityUid User, bool Force);

/// <summary>
/// Raised directed on an item before a user tries to wield it.
/// If this event is cancelled wielding will not happen.
/// </summary>
[ByRefEvent]
public record struct WieldAttemptEvent(EntityUid User, bool Cancelled = false)
{
    public void Cancel()
    {
        Cancelled = true;
    }
}

/// <summary>
/// Raised directed on an item before a user tries to stop wielding it willingly.
/// If this event is cancelled unwielding will not happen.
/// </summary>
/// <remarks>
/// This event is not raised if the user is forced to unwield the item.
/// </remarks>
[ByRefEvent]
public record struct UnwieldAttemptEvent(EntityUid User, bool Cancelled = false)
{
    public void Cancel()
    {
        Cancelled = true;
    }
}