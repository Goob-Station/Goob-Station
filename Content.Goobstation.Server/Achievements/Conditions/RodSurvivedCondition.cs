using Content.Server.ImmovableRod;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class RodSurvivedCondition : AchievementCondition;

public sealed partial class RodSurvivedConditionSystem : AchievementConditionSystem<RodSurvivedCondition>
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    private static readonly TimeSpan SurviveDelay = TimeSpan.FromSeconds(5);

    private readonly Dictionary<EntityUid, TimeSpan> _pending = new();
    private readonly List<EntityUid> _toRemove = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(Entity<MobStateComponent> ent, ref StartCollideEvent args)
    {
        if (Entries.Count == 0 || !HasComp<ImmovableRodComponent>(args.OtherEntity) || _mobState.IsDead(ent, ent.Comp))
            return;

        _pending.TryAdd(ent, _timing.CurTime + SurviveDelay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pending.Count == 0)
            return;

        var now = _timing.CurTime;
        foreach (var (uid, time) in _pending)
        {
            if (time > now)
                continue;

            _toRemove.Add(uid);

            if (!TerminatingOrDeleted(uid) && !_mobState.IsDead(uid))
                ProgressAll(uid);
        }

        foreach (var uid in _toRemove)
        {
            _pending.Remove(uid);
        }

        _toRemove.Clear();
    }
}
