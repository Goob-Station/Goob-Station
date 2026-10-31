using Content.Goobstation.Maths.FixedPoint;

namespace Content.Server.Destructible;

public sealed partial class DestructibleSystem
{
    /// <summary>
    /// Trauma - expose <see cref="DestroyedAt"/> to shared code.
    /// </summary>
    public override FixedPoint2 GetDestroyedAt(EntityUid uid)
        => DestroyedAt(uid);
}
