using Content.Shared.Actions;

namespace Content.Trauma.Shared.ShadowDemon;

/// <summary>
/// Raised when the crawl gets activated
/// </summary>
[ByRefEvent]
public record struct ShadowCrawlActivatedEvent(float LightDamageModifier = 1f);

/// <summary>
/// Raised when the crawl gets de-activated
/// </summary>
[ByRefEvent]
public record struct ShadowCrawlDeActivatedEvent(float LightDamageModifier = 1f);

/// <summary>
/// Raised when attempting to shadow crawl
/// </summary>
/// <param name="Cancelled"></param> Are we allowed to crawl?
[ByRefEvent]
public record struct ShadowCrawlAttemptEvent(bool Cancelled = false);

public sealed partial class ShadowCrawlEvent : InstantActionEvent;
