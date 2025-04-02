using System.Numerics;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.DelayedTeleport;

public sealed class DelayedTeleportSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DelayedTeleportComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.Elapsed += frameTime;

            if (comp.Elapsed < comp.Delay)
                continue;

            if (!_entMan.EntityExists(comp.MapUid) ||
                (comp.GridUid != null && !_entMan.EntityExists(comp.GridUid.Value)))
            {
                RemCompDeferred<DelayedTeleportComponent>(uid);
                continue;
            }

            var target = comp.GridUid ?? comp.MapUid;
            var coordinates = new EntityCoordinates(target, Vector2.One);

            _transformSystem.SetCoordinates(uid, coordinates);
            RemCompDeferred<DelayedTeleportComponent>(uid);
        }
    }

    public void ScheduleTeleport(
        EntityUid target,
        EntityUid mapUid,
        EntityUid? gridUid,
        float delay)
    {
        var comp = EnsureComp<DelayedTeleportComponent>(target);
        comp.MapUid = mapUid;
        comp.GridUid = gridUid;
        comp.Delay = delay;
        comp.Elapsed = 0f;
    }
}
