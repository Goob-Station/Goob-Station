using Content.Shared.Coordinates;
using Content.Trauma.Shared.TimedReplace;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Trauma.Server.TimedReplace;

public sealed class TimedReplaceSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TimedReplaceComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<TimedReplaceComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.SpawnTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(ent.Comp.MinTime, ent.Comp.MaxTime));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var toReplace = new List<(EntityUid uid, TimedReplaceComponent comp)>();

        var query = EntityQueryEnumerator<TimedReplaceComponent>();
        while (query.MoveNext(out var uid, out var replace))
        {
            if (_timing.CurTime < replace.SpawnTime)
                continue;

            toReplace.Add((uid, replace));
        }

        foreach (var (uid, replace) in toReplace)
        {
            ReplaceEntity(uid, replace);
        }
    }

    public void ReplaceEntity(EntityUid uid, TimedReplaceComponent? replace)
    {
        if (!Resolve(uid, ref replace))
            return;

        SpawnAtPosition(replace.Entity, uid.ToCoordinates());
        Del(uid);
    }
}
