using System.Numerics;

namespace Content.Goobstation.Common.MomentumSteering;

public abstract class CommonMomentumSteeringSystem : EntitySystem
{
    public void KillFriction(MomentumSteeringComponent comp, Vector2 velocity, ref float friction)
    {
        var speed = velocity.Length();
        if (speed <= comp.SpeedThreshold)
            return;

        var speedFactor = MathHelper.Clamp(
            (speed - comp.SpeedThreshold) / (comp.MaxSpeed - comp.SpeedThreshold),
            0f,
            1f);
        friction *= MathHelper.Lerp(1f, comp.FrictionReductionAtSpeed, speedFactor);
    }

    public bool TryAdjustedWishDir(
        MomentumSteeringComponent comp,
        Vector2 velocity,
        Vector2 wishDir,
        out Vector2 adjustedWishDir,
        out float speed)
    {
        speed = velocity.Length();
        adjustedWishDir = wishDir;

        if (speed <= comp.SpeedThreshold)
            return false;

        var speedFactor = MathHelper.Clamp(
            (speed - comp.SpeedThreshold) / (comp.MaxSpeed - comp.SpeedThreshold),
            0f,
            1f);
        var steeringPenalty = MathHelper.Lerp(1f, comp.MinSteeringFactor, speedFactor);

        var velNorm = velocity.Normalized();
        var forwardDot = Vector2.Dot(wishDir, velNorm);
        var forwardDir = velNorm * forwardDot;
        var perpDir = wishDir - forwardDir;

        if (forwardDot > 0f)
            adjustedWishDir = forwardDir + perpDir * steeringPenalty;
        else
            adjustedWishDir = forwardDir * comp.BrakingFactor + perpDir * comp.BrakingFactor;

        return true;
    }

    public abstract void TryApplyMomentumJitter(EntityUid uid, MomentumSteeringComponent comp, float speed);
}
