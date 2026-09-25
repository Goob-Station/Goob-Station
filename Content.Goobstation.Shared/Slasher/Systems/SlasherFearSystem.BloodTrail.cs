using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Chemistry.Components;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// The blood trail side of the fear system.
/// </summary>
public sealed partial class SlasherFearSystem
{
    private readonly Dictionary<EntityUid, TimeSpan> _nextDropAt = new();

    private void UpdateBloodTrailState(Entity<SlasherFearComponent> ent)
    {
        var (uid, comp) = ent;

        var bleeding = comp.CurrentMeter >= comp.BloodMeterThreshold;
        if (comp.IsActive == bleeding)
            return;

        comp.IsActive = bleeding;
        Dirty(uid, comp);
    }

    private void StopBloodTrail(EntityUid slasher)
    {
        _nextDropAt.Remove(slasher);
    }

    private void UpdateBloodTrails(TimeSpan now)
    {
        if (!_net.IsServer)
            return;

        var enumerator = EntityQueryEnumerator<SlasherFearComponent>();
        while (enumerator.MoveNext(out var uid, out var comp))
        {
            if (!comp.IsActive)
            {
                _nextDropAt.Remove(uid);
                continue;
            }

            if (!_nextDropAt.TryGetValue(uid, out var next))
            {
                _nextDropAt[uid] = now;
                continue;
            }

            if (now < next)
                continue;

            _nextDropAt[uid] = now + comp.DropInterval;

            var solution = new Solution();
            var amount = FixedPoint2.Max(FixedPoint2.Zero, comp.VolumePerDrop);
            solution.AddReagent(comp.BloodTrailReagent, amount);

            _puddles.TrySpillAt(uid, solution, out _, sound: false);
        }
    }
}
