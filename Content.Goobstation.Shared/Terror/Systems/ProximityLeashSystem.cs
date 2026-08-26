using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Leashed entites cannot go too far away from their leash anchors.
/// If they do, starts counting up a tick and popping up a warning, and gibs on final tick.
/// </summary>
public sealed class ProximityLeashSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var leashQuery = EntityQueryEnumerator<ProximityLeashComponent>();

        while (leashQuery.MoveNext(out var uid, out var leash))
        {
            if (_timing.CurTime < leash.NextTick) continue;

            leash.NextTick = _timing.CurTime + leash.TickInterval;

            var anchor = FindNearestAnchor(uid, leash.LeashGroup);

            if (anchor is null) continue;

            var dist = Vector2.Distance(_xform.GetWorldPosition(uid), _xform.GetWorldPosition(anchor.Value));

            if (dist <= leash.MaxDistance)
            {
                leash.TickCounter = 0;
                continue;
            }

            leash.TickCounter++;

            _popup.PopupEntity(Loc.GetString("terror-leash-straying"), uid, uid, PopupType.Medium);

            if (leash.BreakThreshold > 0 && leash.TickCounter >= leash.BreakThreshold)
            {
                _body.GibBody(uid);
            }
        }
    }

    // Find nearest anchor, return null if none found.
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
            if (anchor.LeashGroup != leashGroup) continue;

            var anchorXform = Transform(anchorUid);

            if (anchorXform.MapID != leashedMap) continue;

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
