using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

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

/// <summary>
/// Action event that shoots a grapple at the direction of clicking.
/// TODO: Move it to its own file
/// </summary>
public sealed partial class ShootGrappleEvent : EntityTargetActionEvent
{
    /// <summary>
    /// The projectile to shoot
    /// </summary>
    [DataField]
    public EntProtoId ProjectileProto;

    /// <summary>
    /// The joint sprite of the projectile (the huge rope that will be attached to the projectile)
    /// </summary>
    [DataField]
    public SpriteSpecifier? JointSprite;
};
