using Content.Goobstation.Common.Surgery;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class OrganTransplantCondition : AchievementCondition;

public sealed class OrganTransplantConditionSystem : AchievementConditionSystem<OrganTransplantCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, OrganTransplantedEvent>(OnTransplanted);
    }

    private void OnTransplanted(Entity<ActorComponent> ent, ref OrganTransplantedEvent args)
    {
        ProgressAll(ent);
    }
}
