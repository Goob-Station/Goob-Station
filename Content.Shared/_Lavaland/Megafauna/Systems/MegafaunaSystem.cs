using Content.Shared._Lavaland.Aggression;

using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna.Systems;

public sealed partial class MegafaunaSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private EntityQuery<AggressiveComponent> _aggressiveQuery;

    public override void Initialize()
    {
        base.Initialize();

        InitializeHandle();

        _aggressiveQuery = GetEntityQuery<AggressiveComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<MegafaunaAiComponent>();
        while (query.MoveNext(out var uid, out var ai))
        {
            if (!ai.Active)
                continue;

            var watch = new Stopwatch();
            watch.Start();
            var selectors = ai.Schedule;
            foreach (var (time, action) in selectors)
            {
                if (time > _timing.CurTime)
                    continue;

                var args = new MegafaunaCalculationBaseArgs(uid, EntityManager, _protoMan, GetRandom());
                var actionTime = action.Invoke(args);
                ai.Schedule.Remove(time);

                // Add next action to this thread
                actionTime = Math.Abs(actionTime); // Safety measure from YAMLmalders.
                var delayTime = ai.ActionDelaySelector.Get(args);
                var nextAction = _timing.CurTime + TimeSpan.FromSeconds(actionTime + delayTime);
                ai.Schedule.Add(nextAction, ai.Selector);
            }

            Log.Info($"Megafauna updated in {watch.Elapsed.TotalMicroseconds} microseconds");
        }
    }

    private System.Random GetRandom()
    {
        return new System.Random((int) _timing.CurTick.Value);
    }
}
