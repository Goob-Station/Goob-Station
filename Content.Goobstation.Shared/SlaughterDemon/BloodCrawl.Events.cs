// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;

namespace Content.Goobstation.Shared.SlaughterDemon;

// Triggers once the slaughter demon activates the Blood Crawl ability while not in Jaunt form.
[ByRefEvent]
public record struct BloodCrawlAttemptEvent(bool Cancelled = false);

// Triggers once the slaughter demon exits the Blood Crawl ability.
[ByRefEvent]
public record struct BloodCrawlExitEvent(bool Cancelled = false);
// Triggers once the slaughter demon enters the Blood Crawl ability in jaunt.
[ByRefEvent]
public record struct BloodCrawlEnterEvent(bool Cancelled = false);
// Triggers once an entity devours another entity
[ByRefEvent]
public record struct SlaughterDevourEvent(EntityUid pullingEnt);
