using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Terror.Systems;

/// <summary>
/// Grows a spiderling on timer end.
/// </summary>
public sealed class TerrorSpiderlingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorSpiderlingComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, TerrorSpiderlingComponent comp, MapInitEvent args)
    {
        comp.GrowAt = _timing.CurTime + comp.GrowDelay;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TerrorSpiderlingComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.GrowAt) continue;

            Grow(uid, comp.GrowsInto);
        }
    }

    private void Grow(EntityUid uid, ProtoId<TerrorSpiderPrototype> growsInto)
    {
        if (!_proto.TryIndex(growsInto, out var spiderProto) || spiderProto.MobPrototype is not { } mobProto)
        {
            QueueDel(uid);
            return;
        }

        var coords = Transform(uid).Coordinates;
        Spawn(mobProto, coords);
        QueueDel(uid);
    }
}
