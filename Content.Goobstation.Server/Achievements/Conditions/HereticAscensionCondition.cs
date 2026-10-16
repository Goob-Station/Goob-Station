using Content.Server.Heretic.EntitySystems;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Mind;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class HereticAscensionCondition : AchievementCondition
{
    [DataField]
    public string? Path;
}

public sealed class HereticAscensionConditionSystem : AchievementConditionSystem<HereticAscensionCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindComponent, EventHereticAscension>(OnAscension, after: [typeof(HereticSystem)]);
    }

    private void OnAscension(Entity<MindComponent> ent, ref EventHereticAscension args)
    {
        if (ent.Comp.CurrentEntity is not { } body
            || !TryComp<HereticComponent>(ent, out var heretic)
            || !heretic.Ascended)
            return;

        foreach (var (proto, condition) in Entries)
        {
            if (condition.Path == null || condition.Path == heretic.CurrentPath)
                Achievements.AddProgress(body, proto);
        }
    }
}
