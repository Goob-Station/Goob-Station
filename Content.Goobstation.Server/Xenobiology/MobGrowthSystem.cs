using System.Linq;
using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles mob growth between development stages.
/// </summary>
public sealed class MobGrowthSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);
    private TimeSpan _nextUpdateTime;

    public override void Initialize()
    {
        base.Initialize();
        _nextUpdateTime = _gameTiming.CurTime + _updateInterval;
    }

    //Mob growth doesn't need to be checked for every frame.
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_nextUpdateTime > _gameTiming.CurTime)
            return;

        _nextUpdateTime = _gameTiming.CurTime + _updateInterval;
        UpdateMobGrowth();
    }

    // Checks entity hunger thresholds, if the threshold required by MobGrowth is met -> grow.
    private void UpdateMobGrowth()
    {
        var eligibleMobs = new HashSet<Entity<MobGrowthComponent, HungerComponent>>();

        var query = EntityQueryEnumerator<MobGrowthComponent>();
        while (query.MoveNext(out var uid, out var growth))
        {
            if (_mobState.IsDead(uid)
                || !TryComp<HungerComponent>(uid, out var hungerComp))
                continue;

            eligibleMobs.Add((uid, growth, hungerComp));
        }

        foreach (var ent in eligibleMobs)
        {
            if (_hunger.GetHunger(ent) < ent.Comp1.HungerRequired
                || ent.Comp1.CurrentStage == ent.Comp1.Stages.LastOrDefault()
                || ent.Comp1.CurrentStage == ent.Comp1.Stages[^1])
                continue;

            DoGrowth(ent);
        }
    }

    #region Helpers

    // Fairly barebones at the moment, this could be expanded to increase HP etc...
    private void DoGrowth(Entity<MobGrowthComponent, HungerComponent> ent)
    {
        if (TerminatingOrDeleted(ent))
            return;

        var currentStage = ent.Comp1.CurrentStage;
        var stages = ent.Comp1.Stages;
        var currentIndex = stages.IndexOf(currentStage);
        var nextIndex = (currentIndex + 1) % stages.Count;
        var nextStage = stages[nextIndex];

        ent.Comp1.CurrentStage = nextStage;

        _hunger.ModifyHunger(ent, ent.Comp1.GrowthCost, ent.Comp2);
        _appearance.SetData(ent, GrowthStateVisuals.Stage, ent.Comp1.CurrentStage);

        Dirty(ent);
    }
    #endregion
}
