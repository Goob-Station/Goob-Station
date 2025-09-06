// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SlaughterDemon;

/// <summary>
/// Triggers once the slaughter demon activates the Blood Crawl ability while not in Jaunt form.
/// </summary>
[ByRefEvent]
public record struct BloodCrawlAttemptEvent(bool Cancelled = false);

/// <summary>
/// Triggers once the slaughter demon exits the Blood Crawl ability.
/// </summary>
[ByRefEvent]
public record struct BloodCrawlExitEvent(bool Cancelled = false);

/// <summary>
/// Triggers once the slaughter demon enters the Blood Crawl ability in jaunt.
/// </summary>
[ByRefEvent]
public record struct BloodCrawlEnterEvent(bool Cancelled = false);

/// <summary>
/// Triggers once an entity devours another entity
/// </summary>
[ByRefEvent]
public record struct SlaughterDevourEvent(EntityUid pullingEnt, EntityCoordinates PreviousCoordinates);

/// <summary>
/// Doafter for when an entity attempts to devour an entity
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SlaughterDevourDoAfter : SimpleDoAfterEvent;
