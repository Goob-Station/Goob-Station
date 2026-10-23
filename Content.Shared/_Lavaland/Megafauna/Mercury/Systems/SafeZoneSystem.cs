using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

public sealed class SafeZoneSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        var query = EntityQueryEnumerator<SafeZoneComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (_timing.CurTime < comp.NextLookupTime)
                continue;

            comp.NextLookupTime = _timing.CurTime + comp.LookupInterval;

            // hashset for fast lookup
            var blacklist = new HashSet<string>(comp.Blacklist.Count);
            foreach (var proto in comp.Blacklist)
            {
                blacklist.Add(proto.Id);
            }

            var nearby = new HashSet<EntityUid>();
            _lookup.GetEntitiesInRange(xform.Coordinates, comp.SafeRadius, nearby);

            foreach (var nearby_uid in nearby)
            {
                if (nearby_uid == uid)
                    continue;

                var meta = MetaData(nearby_uid);
                if (meta.EntityPrototype is null)
                    continue;

                if (blacklist.Contains(meta.EntityPrototype.ID))
                    QueueDel(nearby_uid);
            }
        }
    }
}
