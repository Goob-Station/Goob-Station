using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using System.Numerics;

public sealed class NestGuardSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var guardQuery = EntityQueryEnumerator<PurpleTerrorComponent>();

        while (guardQuery.MoveNext(out var uid, out var guard))
        {
            guard.DamageAccumulator += frameTime;

            var queen = FindNearestQueen(uid);
            if (queen is null)
                continue;

            var dist = Vector2.Distance(
                _xform.GetWorldPosition(uid),
                _xform.GetWorldPosition(queen.Value));

            // Reset when back in range
            if (dist <= guard.MaxDistance)
            {
                guard.DamageAccumulator = 0;
                guard.DeathCounter = 0;
                continue;
            }

            // Tick interval
            if (guard.DamageAccumulator < guard.DamageInterval.TotalSeconds)
                continue;

            guard.DamageAccumulator = 0;
            guard.DeathCounter++;

            _popup.PopupEntity(Loc.GetString("terror-far-from-queen"), uid, uid, PopupType.MediumCaution);

            if (guard.DeathCounter >= 15)
            {
                _body.GibBody(uid);
            }
        }
    }

    private EntityUid? FindNearestQueen(EntityUid guard)
    {
        EntityUid? nearest = null;
        var bestDist = float.MaxValue;
        var guardPos = _xform.GetWorldPosition(guard);

        var query = EntityQueryEnumerator<TerrorQueenComponent>();
        while (query.MoveNext(out var queenUid, out _))
        {
            var dist = Vector2.Distance(guardPos, _xform.GetWorldPosition(queenUid));
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = queenUid;
            }
        }

        return nearest;
    }
}
