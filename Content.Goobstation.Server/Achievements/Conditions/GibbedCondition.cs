using Content.Shared.Body.Components;
using Content.Shared.Body.Events;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class GibbedCondition : AchievementCondition;

public sealed class GibbedConditionSystem : AchievementConditionSystem<GibbedCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, BeingGibbedEvent>(OnGibbed);
    }

    private void OnGibbed(Entity<BodyComponent> ent, ref BeingGibbedEvent args)
    {
        ProgressAll(ent);
    }
}
