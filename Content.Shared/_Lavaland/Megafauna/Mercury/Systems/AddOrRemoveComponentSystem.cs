using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Adds or removes a component, optionally with a timer.
/// Does not support multiple timers so don't even try it.
/// </summary>
public sealed class AddOrRemoveComponentSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AddOrRemoveComponentComponent, AddComponentActionEvent>(OnAddAction);
        SubscribeLocalEvent<AddOrRemoveComponentComponent, RemoveComponentActionEvent>(OnRemoveAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AddOrRemoveComponentComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.RemoveAfterTimer)
                continue;

            if (_timing.CurTime < comp.Accumulator)
                continue;

            TryRemove(uid, comp);
        }
    }

    private void OnAddAction(Entity<AddOrRemoveComponentComponent> ent, ref AddComponentActionEvent args)
    {
        if (args.Handled)
            return;

        var comp = ent.Comp;
        var uid = args.Performer;

        TryAdd(uid, comp, args);

        args.Handled = true;
    }

    private void TryAdd(EntityUid uid, AddOrRemoveComponentComponent comp, AddComponentActionEvent args)
    {
        // Whoopsies I fucked up.
        comp.TargetComponent = args.TargetComponent;
        comp.RemoveAfterTimer = args.RemoveAfterTimer;
        comp.TimeToRemoval = args.TimeToRemoval;

        Dirty(uid, comp);
        // End of fuckup.

        EntityManager.AddComponents(uid, comp.TargetComponent);

        if (args.RemoveAfterTimer)
            comp.Accumulator = _timing.CurTime + comp.TimeToRemoval;
    }
    private void OnRemoveAction(Entity<AddOrRemoveComponentComponent> ent, ref RemoveComponentActionEvent args)
    {
        if (args.Handled)
            return;

        var comp = ent.Comp;
        var uid = ent.Owner;

        comp.TargetComponent = args.TargetComponent;
        Dirty(uid, comp);

        TryRemove(uid, comp);

        args.Handled = true;
    }

    private void TryRemove(EntityUid uid, AddOrRemoveComponentComponent comp)
    {
        if (comp.TargetComponent != null)
        {
            EntityManager.RemoveComponents(uid, comp.TargetComponent);
        }

        // So it doesn't endlessly try to remove it.
        comp.RemoveAfterTimer = false;
        comp.Accumulator = TimeSpan.Zero;

        Dirty(uid, comp);
    }
}
