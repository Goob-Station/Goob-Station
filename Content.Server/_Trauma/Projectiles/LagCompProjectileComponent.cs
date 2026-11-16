using Robust.Shared.Player;

namespace Content.Server._Trauma.Projectiles;

/// <summary>
/// Applies lag compensation to a projectile, allowing it to hit targets based on
/// where they were when the shooter saw them according to their ping.
/// </summary>
[RegisterComponent, Access(typeof(LagCompProjectileSystem))]
public sealed partial class LagCompProjectileComponent : Component
{
    /// <summary>
    /// The player that shot this projectile.
    /// </summary>
    [ViewVariables]
    public ICommonSession? ShooterSession;

    /// <summary>
    /// Entities currently being considered for lag compensated collision.
    /// </summary>
    /// <remarks>
    /// No point being a datafield because the session won't persist, it's useless without it.
    /// </remarks>
    [ViewVariables]
    public HashSet<EntityUid> Targets = new();
}
