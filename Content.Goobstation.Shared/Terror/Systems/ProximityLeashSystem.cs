using Content.Goobstation.Shared.Terror.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Goobstation.Shared.Terror;

/// <summary>
/// Keeps leashed entities from going too far from their anchor.
/// Every few seconds while out of range, a tick fires. Stay far for too long
/// and the leash "breaks". Can be used for gibbing or just dealing damage over time.
/// </summary>
public sealed class ProximityLeashSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var leashQuery = EntityQueryEnumerator<ProximityLeashComponent>();
        while (leashQuery.MoveNext(out var uid, out var leash))
        {
            leash.DamageAccumulator += frameTime;

            var anchor = FindNearestAnchor(uid, leash.LeashGroup);
            if (anchor is null)
                continue;

            var dist = Vector2.Distance(
                _xform.GetWorldPosition(uid),
                _xform.GetWorldPosition(anchor.Value));

            if (dist <= leash.MaxDistance)
            {
                leash.DamageAccumulator = 0;
                leash.TickCounter = 0;
                Dirty(uid, leash);
                continue;
            }

            if (leash.DamageAccumulator < leash.TickInterval.TotalSeconds)
                continue;

            leash.DamageAccumulator = 0;
            leash.TickCounter++;
            Dirty(uid, leash);

            var tickEv = new ProximityLeashTickEvent(uid, anchor.Value, leash.TickCounter);
            RaiseLocalEvent(uid, ref tickEv);

            if (leash.BreakThreshold > 0 && leash.TickCounter >= leash.BreakThreshold)
            {
                var breakEv = new ProximityLeashBreakEvent(uid, anchor.Value);
                RaiseLocalEvent(uid, ref breakEv);
            }
        }
    }

    /// <summary>
    /// Finds the nearest <see cref="ProximityLeashAnchorComponent"/> on the same map as <paramref name="leashed"/>.
    /// Returns null if no valid anchor exists.
    /// </summary>
    private EntityUid? FindNearestAnchor(EntityUid leashed, string leashGroup)
    {
        EntityUid? nearest = null;
        var bestDist = float.MaxValue;

        var leashedXform = Transform(leashed);
        var leashedMap = leashedXform.MapID;
        var leashedPos = _xform.GetWorldPosition(leashed);

        var anchorQuery = EntityQueryEnumerator<ProximityLeashAnchorComponent>();
        while (anchorQuery.MoveNext(out var anchorUid, out var anchor))
        {
            if (anchor.LeashGroup != leashGroup)
                continue;

            var anchorXform = Transform(anchorUid);
            if (anchorXform.MapID != leashedMap)
                continue;

            var dist = Vector2.Distance(leashedPos, _xform.GetWorldPosition(anchorUid));
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = anchorUid;
            }
        }

        return nearest;
    }
}

/// <summary>
/// Raised on a leashed entity each time its out-of-range tick interval fires.
/// </summary>
[ByRefEvent]
public record struct ProximityLeashTickEvent(EntityUid Leashed, EntityUid NearestAnchor, int TickCount);

/// <summary>
/// Raised on a leashed entity when <see cref="ProximityLeashComponent.BreakThreshold"/> ticks are reached.
/// </summary>
[ByRefEvent]
public record struct ProximityLeashBreakEvent(EntityUid Leashed, EntityUid NearestAnchor);
