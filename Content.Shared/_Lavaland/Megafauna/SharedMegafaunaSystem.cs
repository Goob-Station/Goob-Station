using Content.Shared._Lavaland.Aggression;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna;

public abstract class SharedMegafaunaSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaAiComponent, AggressorAddedEvent>(OnAggressorAdded);
        SubscribeLocalEvent<MegafaunaAiComponent, AggressorRemovedEvent>(OnAggressorRemoved);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MegafaunaAiComponent>();
        while (query.MoveNext(out var uid, out var ai))
        {
            if (ai.NextAction < Timing.CurTime
                || !ai.Active)
                continue;

            var args = new MegafaunaCalculationBaseArgs(uid, ai, EntityManager);

            // Invoke action, write that we used it and go on to delay.
            // Hopefully all of this should work the same way on both Client and Server sides
            if (!ai.ActionQueue.TryDequeue(out var attack))
            {
                Logger.Warning($"Failed to Dequeue an attack for {ToPrettyString(uid)}");
                continue;
            }

            var delayTime = attack.Invoke(args);

            ai.PreviousAttack = null;
            if (!ai.CanRepeatAttacks)
                ai.PreviousAttack = attack.Name;

            delayTime = Math.Clamp(delayTime, ai.MinAttackCooldown, ai.MaxAttackCooldown);
            ai.NextAction = Timing.CurTime + TimeSpan.FromSeconds(delayTime);
        }
    }

    #region Event Handling

    private void OnAggressorAdded(Entity<MegafaunaAiComponent> ent, ref AggressorAddedEvent args)
    {
        if (ent.Comp.Active)
            return;

        ent.Comp.Active = true;
        RaiseLocalEvent(ent, new MegafaunaStartupEvent());
    }

    private void OnAggressorRemoved(Entity<MegafaunaAiComponent> ent, ref AggressorRemovedEvent args)
    {
        if (!ent.Comp.Active
            || !TryComp<AggressiveComponent>(ent, out var aggressive)
            || aggressive.Aggressors.Count != 0)
            return;

        ent.Comp.Active = false;
        RaiseLocalEvent(ent, new MegafaunaShutdownEvent());
    }

    #endregion
}
