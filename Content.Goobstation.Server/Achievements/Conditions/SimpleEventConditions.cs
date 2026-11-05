using Content.Goobstation.Common.Abductor;
using Content.Goobstation.Common.Materials;
using Content.Goobstation.Common.Nuke;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Goobstation.Shared.Supermatter.Events;
using Content.Shared.Body.Components;
using Content.Shared.Chasm;
using Content.Shared.Silicons.Laws;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class SupermatterConsumedCondition : AchievementCondition;

public sealed partial class SiliconEmaggedCondition : AchievementCondition;

public sealed partial class AbductedAgainCondition : AchievementCondition;

public sealed partial class NukeDisarmCondition : AchievementCondition;

public sealed partial class ChasmFallCondition : AchievementCondition;

public sealed partial class RecyclerGibbedCondition : AchievementCondition;

public sealed partial class Performed65Condition : AchievementCondition;

public sealed partial class SupermatterConsumedConditionSystem : AchievementConditionSystem<SupermatterConsumedCondition>
{
    [SubscribeLocalEvent]
    private void OnSupermatterConsumedEvent(Entity<ActorComponent> ent, ref SupermatterConsumedEvent args)
    {
        ProgressAll(ent);
    }
}

public sealed partial class SiliconEmaggedConditionSystem : AchievementConditionSystem<SiliconEmaggedCondition>
{
    [SubscribeLocalEvent]
    private void OnSiliconEmaggedEvent(Entity<ActorComponent> ent, ref SiliconEmaggedEvent args)
    {
        ProgressAll(ent);
    }
}

public sealed partial class AbductedAgainConditionSystem : AchievementConditionSystem<AbductedAgainCondition>
{
    [SubscribeLocalEvent]
    private void OnAbductedAgainEvent(Entity<ActorComponent> ent, ref AbductedAgainEvent args)
    {
        ProgressAll(ent);
    }
}

public sealed partial class NukeDisarmConditionSystem : AchievementConditionSystem<NukeDisarmCondition>
{
    [SubscribeLocalEvent]
    private void OnNukeDisarmedByEvent(Entity<ActorComponent> ent, ref NukeDisarmedByEvent args)
    {
        ProgressAll(ent);
    }
}

public sealed partial class ChasmFallConditionSystem : AchievementConditionSystem<ChasmFallCondition>
{
    [SubscribeLocalEvent]
    private void OnComponentStartup(Entity<ChasmFallingComponent> ent, ref ComponentStartup args)
    {
        ProgressAll(ent);
    }
}

public sealed partial class RecyclerGibbedConditionSystem : AchievementConditionSystem<RecyclerGibbedCondition>
{
    [SubscribeLocalEvent]
    private void OnRecyclerGibbedEvent(Entity<BodyComponent> ent, ref RecyclerGibbedEvent args)
    {
        ProgressAll(ent);
    }
}

public sealed partial class Performed65ConditionSystem : AchievementConditionSystem<SupermatterConsumedCondition>
{
    [SubscribeLocalEvent]
    private void OnPerformed65(Entity<ActorComponent> ent, ref Performed65Event args)
    {
        ProgressAll(ent);
        ProgressAll(args.PerformedOn);
    }
}
