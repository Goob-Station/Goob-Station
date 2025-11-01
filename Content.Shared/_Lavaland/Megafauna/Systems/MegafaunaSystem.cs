using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Selectors;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna.Systems;

public sealed partial class MegafaunaSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    private EntityQuery<AggressiveComponent> _aggressiveQuery;

    protected override string SawmillName => "megafauna";

    public override void Initialize()
    {
        base.Initialize();

        InitializeHandle();

        _aggressiveQuery = GetEntityQuery<AggressiveComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // We are not ready to predict this thing... Probably never are.
        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<MegafaunaAiComponent>();
        while (query.MoveNext(out var uid, out var ai))
        {
            if (!ai.Active)
                continue;

            // TODO when there's more than just hierophant make this a CPU job or make it parallel idk whatever is faster
            var selectors = new Dictionary<TimeSpan, MegafaunaSelector>(ai.Schedule);
            foreach (var (time, action) in selectors)
            {
                if (time > _timing.CurTime)
                    continue;

                var args = new MegafaunaCalculationBaseArgs(uid, EntityManager, _protoMan, Log, GetRandom());
                var actionTime = action.Invoke(args);
                ai.Schedule.Remove(time);

                // Add next action to this thread
                actionTime = Math.Max(Math.Abs(actionTime), 0.02f);
                var delayTime = ai.ActionDelaySelector.Get(args);
                var nextAction = _timing.CurTime + TimeSpan.FromSeconds(actionTime + delayTime);
                ai.Schedule.Add(nextAction, ai.Selector);
            }
        }
    }

    // TODO replace this with shared random
    private System.Random GetRandom()
    {
        return new System.Random((int) (_timing.CurTick.Value * 6.7f));
    }
}
