using Content.Server.Body.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.ImpendingDoom;

public sealed class ImpendingDoomSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly BodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DoomedComponent, ComponentInit>(OnComponentInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var curtime = _timing.CurTime;
        var e = EntityQueryEnumerator<DoomedComponent>();

        while (e.MoveNext(out var uid, out var comp))
            if (curtime - comp.AppliedAt > comp.DoomAt)
                Gib(uid);
    }

    private void OnComponentInit(Entity<DoomedComponent> ent, ref ComponentInit args)
    {
        ent.Comp.AppliedAt = _timing.CurTime;
        ent.Comp.DoomAt = TimeSpan.FromMinutes(_rand.Next(2, 60));
    }

    private void Gib(EntityUid ent)
    {
        _body.GibBody(ent);
    }
}
