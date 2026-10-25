using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared.Destructible;

/// <summary>
/// Trauma - lets shared code (predicted projectiles) know how much damage destroys an entity.
/// </summary>
public abstract partial class SharedDestructibleSystem
{
    /// <summary>
    /// Figure out how much damage an entity needs to have in order to be destroyed.
    /// Destructible thresholds only exist on the server, so the client always gets <see cref="FixedPoint2.MaxValue"/>.
    /// </summary>
    public virtual FixedPoint2 GetDestroyedAt(EntityUid uid)
        => FixedPoint2.MaxValue;
}
