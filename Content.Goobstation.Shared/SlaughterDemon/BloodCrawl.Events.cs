namespace Content.Goobstation.Shared.SlaughterDemon;

// Triggers once the slaughter demon activates the Blood Crawl ability while not in Jaunt form.
[ByRefEvent]
public record struct BloodCrawlAttemptEvent(bool Cancelled = false);

// Triggers once the slaughter demon exits the Blood Crawl ability.
[ByRefEvent]
public record struct BloodCrawlExitEvent(bool Cancelled = false);
