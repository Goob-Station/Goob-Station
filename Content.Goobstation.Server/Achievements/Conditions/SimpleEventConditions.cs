using Content.Goobstation.Common.Abductor;
using Content.Goobstation.Common.Materials;
using Content.Goobstation.Common.MisandryBox;
using Content.Goobstation.Common.Nuke;
using Content.Goobstation.Shared.Supermatter.Events;
using Content.Shared.Body.Components;
using Content.Shared.Chasm;
using Content.Shared.Silicons.Laws;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class SupermatterConsumedCondition : AchievementCondition;

public sealed partial class SiliconEmaggedCondition : AchievementCondition;

public sealed partial class EmoteSpamSmitedCondition : AchievementCondition;

public sealed partial class AbductedAgainCondition : AchievementCondition;

public sealed partial class NukeDisarmCondition : AchievementCondition;

public sealed partial class ChasmFallCondition : AchievementCondition;

public sealed partial class RecyclerGibbedCondition : AchievementCondition;

public sealed class SupermatterConsumedConditionSystem : AchievementConditionSystem<SupermatterConsumedCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, SupermatterConsumedEvent>(OnSupermatterConsumedEvent);
    }

    private void OnSupermatterConsumedEvent(Entity<ActorComponent> ent, ref SupermatterConsumedEvent args)
    {
        ProgressAll(ent);
    }
}

public sealed class SiliconEmaggedConditionSystem : AchievementConditionSystem<SiliconEmaggedCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, SiliconEmaggedEvent>(OnSiliconEmaggedEvent);
    }

    private void OnSiliconEmaggedEvent(Entity<ActorComponent> ent, ref SiliconEmaggedEvent args)
    {
        ProgressAll(ent);
    }
}

public sealed class EmoteSpamSmitedConditionSystem : AchievementConditionSystem<EmoteSpamSmitedCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, EmoteSpamSmitedEvent>(OnEmoteSpamSmitedEvent);
    }

    private void OnEmoteSpamSmitedEvent(Entity<ActorComponent> ent, ref EmoteSpamSmitedEvent args)
    {
        ProgressAll(ent);
    }
}

public sealed class AbductedAgainConditionSystem : AchievementConditionSystem<AbductedAgainCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, AbductedAgainEvent>(OnAbductedAgainEvent);
    }

    private void OnAbductedAgainEvent(Entity<ActorComponent> ent, ref AbductedAgainEvent args)
    {
        ProgressAll(ent);
    }
}

public sealed class NukeDisarmConditionSystem : AchievementConditionSystem<NukeDisarmCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, NukeDisarmedByEvent>(OnNukeDisarmedByEvent);
    }

    private void OnNukeDisarmedByEvent(Entity<ActorComponent> ent, ref NukeDisarmedByEvent args)
    {
        ProgressAll(ent);
    }
}

public sealed class ChasmFallConditionSystem : AchievementConditionSystem<ChasmFallCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChasmFallingComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(Entity<ChasmFallingComponent> ent, ref ComponentStartup args)
    {
        ProgressAll(ent);
    }
}

public sealed class RecyclerGibbedConditionSystem : AchievementConditionSystem<RecyclerGibbedCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, RecyclerGibbedEvent>(OnRecyclerGibbedEvent);
    }

    private void OnRecyclerGibbedEvent(Entity<BodyComponent> ent, ref RecyclerGibbedEvent args)
    {
        ProgressAll(ent);
    }
}
