// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Nutrition.Components;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles mob growth between development stages.
/// </summary>
public partial class XenobiologySystem
{
    private readonly TimeSpan _growthInterval = TimeSpan.FromSeconds(1);
    private TimeSpan _nextGrowthTime; // fix this shit dawg

    private void InitializeGrowth()
    {
        base.Initialize();
        _nextGrowthTime = _gameTiming.CurTime + _growthInterval;
    }

    //Mob growth doesn't need to be checked for every frame.
    private void UpdateGrowth()
    {
        if (_nextGrowthTime > _gameTiming.CurTime)
            return;

        _nextGrowthTime = _gameTiming.CurTime + _growthInterval;
        UpdateMobGrowth();
    }

    // Checks entity hunger thresholds, if the threshold required by MobGrowth is met -> grow.
    private void UpdateMobGrowth()
    {
        var eligibleMobs = new HashSet<Entity<MobGrowthComponent, HungerComponent>>();

        var query = EntityQueryEnumerator<MobGrowthComponent, HungerComponent>();
        while (query.MoveNext(out var uid, out var growth, out var hungerComp))
        {
            if (_mobState.IsDead(uid))
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
