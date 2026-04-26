using Content.Goobstation.Shared.Devil.Components;
using Content.Shared.Atmos.EntitySystems;
using Robust.Shared.Map.Components;

namespace Content.Goobstation.Shared.Devil.Systems;

public sealed class HellstepSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HellstepComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var hellstep, out var xform))
        {
            hellstep.LifetimeTimer += frameTime;
            if (hellstep.LifetimeTimer >= hellstep.LifetimeDuration)
            {
                RemCompDeferred<HellstepComponent>(uid);
                continue;
            }

            hellstep.SpawnTimer += frameTime;
            if (hellstep.SpawnTimer < hellstep.SpawnInterval)
                continue;

            hellstep.SpawnTimer = 0f;
            SpawnFireBehind(uid, xform, hellstep);
        }
    }

    private void SpawnFireBehind(
        EntityUid uid,
        TransformComponent xform,
        HellstepComponent hellstep)
    {
        if (xform.GridUid is not { } gridUid)
            return;

        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var dir = xform.LocalRotation.GetDir();
        var behind = dir.GetOpposite();

        if (!_xform.TryGetGridTilePosition(uid, out var tilePos))
            return;

        var targetTile = tilePos.Offset(behind);

        var worldPos = _map.GridTileToWorld(
            gridUid,
            grid,
            targetTile
        );

        if (HasComp<ArchdevilComponent>(uid))
        {
            Spawn(hellstep.LavaPrototype, worldPos);
        }

        Spawn(hellstep.FirePrototype, worldPos);
    }

}



