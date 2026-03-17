using System.Numerics;
using Content.Goobstation.Common.MomentumSteering;
using Content.Shared.Jittering;
using Content.Shared.Movement.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MomentumSteering;

public sealed class MomentumSteeringSystem : CommonMomentumSteeringSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;

    private EntityQuery<JetpackUserComponent> _jetpackUserQuery;

    public override void Initialize()
    {
        base.Initialize();
        _jetpackUserQuery = GetEntityQuery<JetpackUserComponent>();
    }

    public override bool TryAdjustedWishDir(
        EntityUid uid,
        MomentumSteeringComponent comp,
        Vector2 velocity,
        Vector2 wishDir,
        out Vector2 adjustedWishDir,
        out float speed,
        float bonusFactor = 0f)
    {
        if (_jetpackUserQuery.HasComp(uid))
            bonusFactor = comp.JetpackSteeringBonus;

        return base.TryAdjustedWishDir(uid, comp, velocity, wishDir, out adjustedWishDir, out speed, bonusFactor);
    }

    public override void TryApplyMomentumJitter(EntityUid uid, MomentumSteeringComponent comp, float speed)
    {
        if (speed < comp.JitterSpeedThreshold)
            return;

        var curTime = _timing.CurTime;
        if (curTime - comp.LastJitterTime < TimeSpan.FromSeconds(0.5))
            return;

        comp.LastJitterTime = curTime;
        Dirty(uid, comp);

        var speedFactor = MathHelper.Clamp(
            (speed - comp.JitterSpeedThreshold) / (comp.MaxSpeed - comp.JitterSpeedThreshold),
            0f, 1f);
        var amplitude = comp.JitterAmplitude * speedFactor;

        _jittering.DoJitter(uid, TimeSpan.FromSeconds(0.6), true, amplitude, comp.JitterFrequency);
    }
}
