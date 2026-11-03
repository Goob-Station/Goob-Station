using Content.Goobstation.Shared.SlotMachine;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class PrizeWonCondition : AchievementCondition
{
    [DataField(required: true)]
    public List<ProtoId<PrizePrototype>> Prizes = new();
}

public sealed class PrizeWonConditionSystem : AchievementConditionSystem<PrizeWonCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, PrizeWonEvent>(OnPrizeWon);
    }

    private void OnPrizeWon(Entity<ActorComponent> ent, ref PrizeWonEvent args)
    {
        foreach (var (proto, condition) in Entries)
        {
            if (condition.Prizes.Contains(args.Prize))
                Achievements.AddProgress(ent, proto);
        }
    }
}
