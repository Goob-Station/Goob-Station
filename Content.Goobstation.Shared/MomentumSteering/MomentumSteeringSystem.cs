using Content.Goobstation.Common.MomentumSteering;
using Content.Shared.Jittering;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MomentumSteering;

public sealed class MomentumSteeringSystem : CommonMomentumSteeringSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;

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
