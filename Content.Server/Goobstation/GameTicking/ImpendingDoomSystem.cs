using System.Linq;
using Content.Server.Administration.Components;
using Content.Server.Body.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Goobstation.GameTicking;

public sealed class ImpendingDoomSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly BodySystem _body = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    private TimeSpan ticker;

    private readonly List<string> _condemned = new() { "MobFelinid" };

    public override void Initialize()
    {
        base.Initialize();

        ticker = TimeSpan.FromMinutes(_rand.Next(4, 10));

        SubscribeLocalEvent<DoomedComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<DoomedComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<BufferingComponent, ComponentRemove>(OnCurseRemove);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Predicting on server???
        if (!_timing.IsFirstTimePredicted)
            return;

        var curtime = _timing.CurTime;
        var e = EntityQueryEnumerator<DoomedComponent>();

        while (e.MoveNext(out var uid, out var comp))
        {
            if (comp.Applied)
                continue;

            if (curtime - comp.AppliedAt > ticker)
            {
                Curse(uid, comp);
            }
        }
    }

    public void O6PE4EH(EntityUid ent)
    {
        var proto = Prototype(ent);

        if (proto is not null && _condemned.Any(mob => mob == proto.ID))
        {
            Condemn(ent);
        }
    }

    private void OnComponentInit(Entity<DoomedComponent> ent, ref ComponentInit args)
    {
        ent.Comp.AppliedAt = _timing.CurTime;
    }

    private void OnComponentRemove(Entity<DoomedComponent> ent, ref ComponentRemove args)
    {
        Explode(ent.Owner);
    }

    private void OnCurseRemove(Entity<BufferingComponent> ent, ref ComponentRemove args)
    {
        // No cheating the hangman
        if (TryComp<DoomedComponent>(ent, out var doom) && doom.Applied)
            Explode(ent.Owner);
    }

    private void Condemn(EntityUid ent)
    {
        EnsureComp<DoomedComponent>(ent);
    }

    private void Curse(EntityUid ent, DoomedComponent comp)
    {
        comp.Applied = true;
        EnsureComp<BufferingComponent>(ent);
    }

    private void Explode(EntityUid ent)
    {
        _explosion.QueueExplosion(ent,
            ExplosionSystem.DefaultExplosionPrototypeId,
            totalIntensity: 4,
            slope: 1,
            maxTileIntensity: 2,
            maxTileBreak: 0,
            canCreateVacuum: false);

        _body.GibBody(ent);
    }
}
