using System.Numerics;
using Content.Goobstation.Common.MomentumSteering;
using Content.Shared.Movement.Components;

namespace Content.Goobstation.Shared.MomentumSteering;

public sealed class MomentumThrustSystem : CommonMomentumThrustSystem
{
    [Dependency] private readonly CommonMomentumSteeringSystem _steering = default!;

    private EntityQuery<JetpackUserComponent> _jetpackUserQuery;

    public override void Initialize()
    {
        base.Initialize();
        _jetpackUserQuery = GetEntityQuery<JetpackUserComponent>();
    }

    public override void AdjustWishDir(EntityUid uid, MomentumSteeringComponent comp, Vector2 originalWishDir, ref Vector2 adjustedWishDir, float speed)
    {
        if (!_jetpackUserQuery.HasComp(uid))
            return;

        adjustedWishDir = Vector2.Lerp(adjustedWishDir, originalWishDir, comp.JetpackSteeringBonus);
    }
}
