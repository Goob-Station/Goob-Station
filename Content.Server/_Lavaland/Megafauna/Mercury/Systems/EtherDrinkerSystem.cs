using Content.Server.Radiation.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared.Timing;
using Robust.Shared.Timing;

namespace Content.Server._Lavaland.Megafauna.Mercury.Systems;

public sealed class EtherDrinkerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<EtherDrinkerComponent, RadiationReceiverComponent, UseDelayComponent>();
        while (query.MoveNext(out var uid, out var comp, out var receiver, out var delay))
        {
            Log.Info($"EtherDrinker {uid}: CurrentRadiation={receiver.CurrentRadiation}");

            if (receiver.CurrentRadiation <= 0f)
                continue;

            if (!_useDelay.TryGetDelayInfo((uid, delay), out var info))
                continue;

            if (info.EndTime <= _timing.CurTime)
                continue;

            var pull = TimeSpan.FromSeconds(receiver.CurrentRadiation * comp.RadiationRechargeMultiplier * frameTime);

            info.EndTime -= pull;

            if (info.EndTime < _timing.CurTime)
            {
                info.EndTime = _timing.CurTime;
            }

            Dirty(uid, delay);
        }
    }
}
