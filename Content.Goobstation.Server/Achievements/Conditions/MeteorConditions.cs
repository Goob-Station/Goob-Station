using Content.Server.Mining;
using Content.Shared.Examine;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class ExamineMeteorCondition : AchievementCondition;

public sealed partial class PunchMeteorCondition : AchievementCondition;

public sealed class ExamineMeteorConditionSystem : AchievementConditionSystem<ExamineMeteorCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MeteorComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<MeteorComponent> ent, ref ExaminedEvent args)
    {
        if (args.IsInDetailsRange)
            ProgressAll(args.Examiner);
    }
}

public sealed class PunchMeteorConditionSystem : AchievementConditionSystem<PunchMeteorCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MeteorComponent, AttackedEvent>(OnAttacked);
    }

    private void OnAttacked(Entity<MeteorComponent> ent, ref AttackedEvent args)
    {
        if (args.Used == args.User)
            ProgressAll(args.User);
    }
}
