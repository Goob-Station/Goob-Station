using Content.Server.Stunnable.Systems;
using Content.Shared.Whitelist;

namespace Content.Server.Stunnable.Components;

/// <summary>
///     Adds stun when it collides with an entity
/// </summary>
[RegisterComponent, Access(typeof(StunOnCollideSystem))]
public sealed partial class StunOnCollideComponent : Component
{
    // TODO: Can probably predict this.

    [DataField]
    public TimeSpan StunAmount = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan KnockdownAmount = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan SlowdownAmount = TimeSpan.FromSeconds(10);

    [DataField]
    public float WalkSpeedMultiplier = 1f;

    [DataField]
    public float RunSpeedMultiplier = 1f;

    /// <summary>
    ///     Fixture we track for the collision.
    /// </summary>
    [DataField]
    public string FixtureId = "projectile";

    /// <summary>
    ///     Entities excluded from collision check.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;
}
