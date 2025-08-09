// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Factory.Filters;

/// <summary>
/// Marker component for filter items.
/// Only used for whitelisting, does nothing on its own.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AutomationFilterComponent : Component;

/// <summary>
/// Event raised on a filter to determine if it should block an item.
/// If <c>CouldAllow</c> is set to true, IsAlwaysBlocked will return false.
/// </summary>
[ByRefEvent]
public record struct AutomationFilterEvent(EntityUid Item, bool Allowed = false, bool CouldAllow = false);

/// <summary>
/// Event raised on a filter to get its stack split size.
/// </summary>
[ByRefEvent]
public record struct AutomationFilterSplitEvent(int Size = 0);
