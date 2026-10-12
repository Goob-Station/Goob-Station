using Content.Server.Nutrition.EntitySystems;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class SmokeCondition : AchievementCondition;

public sealed partial class SmokeConditionSystem : AchievementConditionSystem<SmokeCondition>
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private InventorySystem _inventory = default!;

    private readonly HashSet<EntityUid> _finished = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmokableComponent, SmokableSolutionEmptyEvent>(OnSolutionEmpty, before: [typeof(SmokingSystem)]);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnSolutionEmpty(Entity<SmokableComponent> ent, ref SmokableSolutionEmptyEvent args)
    {
        if (!_container.TryGetContainingContainer((ent, null, null), out var container)
            || !_inventory.TryGetSlotEntity(container.Owner, "mask", out var masked)
            || masked != ent.Owner
            || !_finished.Add(ent))
            return;

        ProgressAll(container.Owner);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _finished.Clear();
    }
}
