using Content.Shared._Lavaland.Aggression;

using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Conditions;
using Content.Shared.Mobs;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna.Systems;

public sealed partial class MegafaunaSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    private EntityQuery<AggressiveComponent> _aggressiveQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaAiComponent, MegafaunaStartupEvent>(OnMegafaunaStartup);
        SubscribeLocalEvent<MegafaunaAiComponent, MegafaunaShutdownEvent>(OnMegafaunaShutdown);
        SubscribeLocalEvent<MegafaunaAiComponent, AggressorAddedEvent>(OnAggressorAdded);
        SubscribeLocalEvent<MegafaunaAiComponent, AggressorRemovedEvent>(OnAggressorRemoved);
        SubscribeLocalEvent<MegafaunaAiComponent, MobStateChangedEvent>(OnStateChanged);

        _aggressiveQuery = GetEntityQuery<AggressiveComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MegafaunaAiComponent>();
        while (query.MoveNext(out var uid, out var ai))
        {
            if (!ai.Active)
                continue;

            var selectors = ai.Schedule;
            foreach (var (time, action) in selectors)
            {
                if (time > _timing.CurTime)
                    continue;

                var args = new MegafaunaCalculationBaseArgs(uid,
                    ai.AiEntity,
                    EntityManager,
                    _protoMan,
                    GetRandom());

                var actionTime = action.Invoke(args);
                ai.Schedule.Remove(time);

                // Add next action to this thread
                actionTime = Math.Abs(actionTime); // Safety measure from YAMLmalders.
                var delayTime = ai.ActionDelaySelector.Get(args);
                var nextAction = _timing.CurTime + TimeSpan.FromSeconds(actionTime + delayTime);
                ai.Schedule.Add(nextAction, ai.Selector);
            }
        }
    }

    public void StartupMegafauna(Entity<MegafaunaAiComponent> ent)
    {
        RaiseLocalEvent(ent, new MegafaunaStartupEvent());
        ent.Comp.Active = true;
    }

    public void ShutdownMegafauna(Entity<MegafaunaAiComponent> ent, bool kill = false)
    {
        RaiseLocalEvent(ent, new MegafaunaShutdownEvent());

        if (kill)
            RaiseLocalEvent(ent, new MegafaunaKilledEvent());

        ent.Comp.Active = false;
    }

    /// <summary>
    /// Picks a new target based on bosses AggressiveComponent.
    /// </summary>
    public bool TryPickTargetAggressive(MegafaunaCalculationBaseArgs args, List<MegafaunaTargetCondition> conditions)
    {
        if (!_aggressiveQuery.TryComp(args.BossEntity, out var aggressiveComp))
            return false;

        var results = new Dictionary<EntityUid, int>();
        foreach (var target in aggressiveComp.Aggressors)
        {
            var fails = 0;
            foreach (var condition in conditions)
            {
                condition.Target = target;
                if (!condition.Evaluate(args))
                    fails++;
            }

            results.Add(target, fails);
        }

        // I bet that this code sucks. If someone actually knows how to code please fix.
        var leastFails = int.MaxValue;
        foreach (var (_, fails) in results)
        {
            if (leastFails > fails)
                leastFails = fails;
        }

        EntityUid? picked = null;
        foreach (var (target, fails) in results)
        {
            if (fails == leastFails)
                picked = target;
        }

        if (picked == null
            || args.AiEntity == null)
            return false;

        var targetComp = EnsureComp<MegafaunaTargetingComponent>(args.AiEntity.Value);
        targetComp.TargetEntity = picked;
        return true;
    }

    private System.Random GetRandom()
    {
        return new System.Random((int) _timing.CurTick.Value);
    }
}
