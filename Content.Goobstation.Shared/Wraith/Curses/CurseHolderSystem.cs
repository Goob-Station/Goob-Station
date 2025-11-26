using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Curses;

public abstract class SharedCurseHolderSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseHolderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CurseHolderComponent, CurseAppliedEvent>(OnApplied);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<CurseHolderComponent>();
        while (eqe.MoveNext(out var uid, out var curseHolder))
        {
            if (curseHolder.ActiveCurses.Count == 0)
                return;

            foreach (var curse in curseHolder.ActiveCurses)
            {
                if (_timing.CurTime < curseHolder.CurseUpdate[curse])
                    continue;

                var ev = new CurseEffectAppliedEvent(curse);
                RaiseLocalEvent(uid, ref ev);

                DoCurseEffects(curse, uid, curseHolder);
            }
        }
    }

    private void OnShutdown(Entity<CurseHolderComponent> ent, ref ComponentShutdown args)
    {
        // remove any components applied from the curse
        foreach (var curse in ent.Comp.ActiveCurses)
        {
            if (!_proto.TryIndex(curse, out var curseIndex)
                || curseIndex.Components == null)
                continue;

            EntityManager.RemoveComponents(ent.Owner, curseIndex.Components);
        }
    }

    private void OnApplied(Entity<CurseHolderComponent> ent, ref CurseAppliedEvent args)
    {
        if (ent.Comp.Curser == null)
            ent.Comp.Curser = args.Curser;

        if (ent.Comp.ActiveCurses.Contains(args.Curse)
            || !_proto.TryIndex(args.Curse, out var curseIndex))
        {
            args.Cancelled = true; // so the actions don't go to cooldown
            return;
        }

        if (curseIndex.Components != null)
            EntityManager.AddComponents(ent.Owner, curseIndex.Components);

        ent.Comp.ActiveCurses.Add(args.Curse);
        ent.Comp.CurseUpdate.Add(args.Curse, _timing.CurTime + TimeSpan.FromSeconds(curseIndex.Update));
        Dirty(ent);

        if (curseIndex.StatusIcon == null)
            return;

        ent.Comp.CurseStatusIcons.Add(curseIndex.StatusIcon.Value);
        Dirty(ent);

        _adminLogger.Add(LogType.Action, LogImpact.Medium,
            $"{ToPrettyString(ent.Comp.Curser)} cursed {ToPrettyString(ent.Owner)} with {curseIndex.Name}");
    }

    #region Helper
    private void DoCurseEffects(ProtoId<CursePrototype> curse, EntityUid target, CurseHolderComponent curseHolder)
    {
        if (_netManager.IsClient)
            return;

        if (!_proto.TryIndex(curse, out var curseIndex))
            return;

        var args = new EntityEffectBaseArgs(target, EntityManager);
        // roll the chance
        foreach (var (chance, curseEffects) in curseIndex.Effects)
        {
            if (_random.Prob(chance))
            {
                foreach (var effect in curseEffects)
                    effect.Effect(args);

                curseHolder.CurseUpdate[curse] = _timing.CurTime + TimeSpan.FromSeconds(curseIndex.Update);
                Dirty(target, curseHolder);
                return; // only do 1 set of effects at a time
            }
        }

        curseHolder.CurseUpdate[curse] = _timing.CurTime + TimeSpan.FromSeconds(curseIndex.Update);
        Dirty(target, curseHolder);
    }
    #endregion
}
