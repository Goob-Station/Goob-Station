// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
