using Content.Goobstation.Shared.Wraith.Components;
using Content.Shared.Examine;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class HauntedSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HauntedComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<HauntedComponent, MapInitEvent>(OnMapInit);
    }

    private void OnExamined(EntityUid uid, HauntedComponent comp, ExaminedEvent args)
    {
        if (HasComp<WraithComponent>(args.Examiner))
        {
            var remaining = comp.DeletionTime - _timing.CurTime;
            args.PushMarkup($"[color=mediumpurple]{Loc.GetString("wraith-already-haunted", ("target", uid))}[/color]");
            args.PushMarkup($"Expires in: {remaining.Minutes}m {remaining.Seconds}s");
        }
    }
    private void OnMapInit(EntityUid uid, HauntedComponent comp, MapInitEvent args)
    {
        comp.DeletionTime = _timing.CurTime + comp.Lifetime;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HauntedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.DeletionTime)
                continue;

            RemCompDeferred<HauntedComponent>(uid);
        }
    }
}
