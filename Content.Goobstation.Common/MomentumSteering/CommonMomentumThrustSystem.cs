using System.Numerics;

namespace Content.Goobstation.Common.MomentumSteering;

public abstract class CommonMomentumThrustSystem : EntitySystem
{
    public abstract void AdjustWishDir(EntityUid uid, MomentumSteeringComponent comp, Vector2 originalWishDir, ref Vector2 adjustedWishDir, float speed);
}
