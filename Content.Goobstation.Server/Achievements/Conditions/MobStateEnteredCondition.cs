using Content.Server.Hands.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Standing;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class MobStateEnteredCondition : AchievementCondition
{
    [DataField]
    public MobState MobState = MobState.Dead;
}

public sealed class MobStateEnteredConditionSystem : AchievementConditionSystem<MobStateEnteredCondition>
{
    private readonly Dictionary<EntityUid, MobState> _handled = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, DropHandItemsEvent>(OnDropHandItems, before: [typeof(HandsSystem)]);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnDropHandItems(Entity<MobStateComponent> ent, ref DropHandItemsEvent args)
    {
        var state = ent.Comp.CurrentState;
        if (state is not (MobState.Critical or MobState.Dead)
            || _handled.TryGetValue(ent, out var handled) && handled == state)
            return;

        _handled[ent] = state;

        foreach (var (proto, condition) in Entries)
        {
            if (condition.MobState == state)
                Achievements.AddProgress(ent, proto);
        }
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        _handled.Remove(args.Target);
    }
}
