using Content.Goobstation.Shared.Devil.Contract;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class DevilContractCondition : AchievementCondition;

public sealed class DevilContractConditionSystem : AchievementConditionSystem<DevilContractCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, DevilContractSignedEvent>(OnSigned);
    }

    private void OnSigned(Entity<ActorComponent> ent, ref DevilContractSignedEvent args)
    {
        ProgressAll(ent);
    }
}
