using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared.Whitelist;

namespace Content.Shared._Lavaland.Megafauna.Systems;

/// <summary>
/// This handles chasing a target and all the logic that entails.
/// </summary>
public sealed class MegafaunaChaseTargetSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<
            MegafaunaChaseTargetComponent,
            MegafaunaAnchorComponent,
            TransformComponent>();

        while (query.MoveNext(out var uid, out var chase, out var anchor, out var xform))
        {
            // So long as the Megafauna is anchored, it will not chase.
            if (anchor.Anchored)
                continue;

            // If false, don't chase.
            if (!chase.ChaseNow)
                continue;

            UpdateChase(uid, chase, xform, frameTime);
        }
    }
    private void UpdateChase(EntityUid uid, MegafaunaChaseTargetComponent chase, TransformComponent xform, float frameTime)
    {
        if (!TryGetTarget(uid, chase, xform, out var targetUid))
            return;

        if (!TryComp<TransformComponent>(targetUid, out var targetXform))
            return;

        var ourPos = _xform.GetWorldPosition(xform);
        var targetPos = _xform.GetWorldPosition(targetXform);

        var delta = targetPos - ourPos;
        var distance = delta.Length();

        if (chase.StopWhenNear && distance <= chase.StopDistance)
            return;

        var direction = delta.Normalized();
        var movement = direction * chase.MovementSpeed * frameTime;

        _xform.SetWorldPosition(uid, ourPos + movement);
    }
    private bool TryGetTarget(EntityUid owner, MegafaunaChaseTargetComponent chase, TransformComponent ownerXform, out EntityUid target)
    {
        target = default;

        var ownerPos = _xform.GetWorldPosition(ownerXform);

        float bestDistSq = float.MaxValue;
        EntityUid bestTarget = default;

        var query = EntityQueryEnumerator<TransformComponent>();

        while (query.MoveNext(out var uid, out var xform))
        {
            // Never target self
            if (uid == owner)
                continue;

            // Must be in the same map
            if (xform.MapID != ownerXform.MapID)
                continue;

            // Blacklist check
            if (chase.Blacklist != null &&
                _whitelist.IsValid(chase.Blacklist, uid))
                continue;

            // Whitelist check
            if (chase.Whitelist != null &&
                !_whitelist.IsValid(chase.Whitelist, uid))
                continue;

            var pos = _xform.GetWorldPosition(xform);
            var delta = pos - ownerPos;
            var distSq = delta.LengthSquared();

            if (distSq >= bestDistSq)
                continue;

            bestDistSq = distSq;
            bestTarget = uid;
        }

        if (bestTarget == default)
            return false;

        chase.Target = bestTarget;
        target = bestTarget;
        return true;
    }

}

