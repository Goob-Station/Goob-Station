using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Slasher;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class ShadowlingAscensionCondition : AchievementCondition;

public sealed partial class SlasherAscensionCondition : AchievementCondition;

public sealed class ShadowlingAscensionConditionSystem : AchievementConditionSystem<ShadowlingAscensionCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingAscendEvent>(OnAscend);
    }

    private void OnAscend(ShadowlingAscendEvent ev)
    {
        ProgressAll(ev.ShadowlingAscended);
    }
}

public sealed class SlasherAscensionConditionSystem : AchievementConditionSystem<SlasherAscensionCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherAscendedEvent>(OnAscend);
    }

    private void OnAscend(SlasherAscendedEvent ev)
    {
        ProgressAll(ev.Slasher);
    }
}
