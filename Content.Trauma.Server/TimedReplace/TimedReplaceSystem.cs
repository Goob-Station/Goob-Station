// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Coordinates;
using Content.Trauma.Shared.TimedReplace;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Trauma.Server.TimedReplace;

public sealed partial class TimedReplaceSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _timing = default!;

    List<Entity<TimedReplaceComponent>> toReplace = new ();

    public override void Initialize()
    {
        SubscribeLocalEvent<TimedReplaceComponent, MapInitEvent>(OnMapInit);
    }
    private void OnMapInit(Entity<TimedReplaceComponent> ent, ref MapInitEvent args)
    {
        if (!ent.Comp.Active)
            return;

        ent.Comp.SpawnTime = _timing.CurTime + ent.Comp.Time;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        toReplace.Clear();

        var query = EntityQueryEnumerator<TimedReplaceComponent>();
        while (query.MoveNext(out var uid, out var replace))
        {
            if (!replace.Active || _timing.CurTime < replace.SpawnTime)
                continue;

            toReplace.Add((uid, replace));
        }

        foreach (var (uid, replace) in toReplace)
        {
            ReplaceEntity(uid, replace);
        }
    }

    private void ReplaceEntity(EntityUid uid, TimedReplaceComponent replace)
    {
        SpawnAtPosition(replace.Entity, Transform(uid).Coordinates);
        Del(uid);
    }
}
