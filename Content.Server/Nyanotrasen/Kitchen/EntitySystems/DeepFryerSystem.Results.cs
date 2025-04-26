using System.Text;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Kitchen.Components;
using Content.Server.Nutrition.Components;
using Content.Server.Nyanotrasen.Kitchen.Components;
using Content.Server.Paper;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Buckle.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nyanotrasen.Kitchen.Components;
using Robust.Shared.Random;

namespace Content.Server.Nyanotrasen.Kitchen.EntitySystems;

public sealed partial class DeepFryerSystem
{
    /// <summary>
    ///     Make an item look deep-fried.
    /// </summary>
    private void MakeCrispy(EntityUid item)
    {
        EnsureComp<AppearanceComponent>(item);
        EnsureComp<DeepFriedComponent>(item);

        _appearanceSystem.SetData(item, DeepFriedVisuals.Fried, true);
    }

    /// <summary>
    ///     Turn a dead mob into food.
    /// </summary>
    /// <remarks>
    ///     This is meant to be an irreversible process, similar to gibbing.
    /// </remarks>
    public bool TryMakeMobIntoFood(EntityUid mob, MobStateComponent mobStateComponent, bool force = false)
    {
        // Don't do anything to mobs until they're dead.
        if (force || _mobStateSystem.IsDead(mob, mobStateComponent))
        {
            RemComp<ActiveNPCComponent>(mob);
            RemComp<AtmosExposedComponent>(mob);
            RemComp<BarotraumaComponent>(mob);
            RemComp<BuckleComponent>(mob);
            RemComp<GhostTakeoverAvailableComponent>(mob);
            RemComp<InternalsComponent>(mob);
            RemComp<PerishableComponent>(mob);
            RemComp<RespiratorComponent>(mob);
            RemComp<RottingComponent>(mob);

            // Ensure it's Food here, so it passes the whitelist.
            var mobFoodComponent = EnsureComp<FoodComponent>(mob);
            _solutionContainerSystem.EnsureSolution(mob, mobFoodComponent.Solution, out var alreadyHadFood);

            if (!_solutionContainerSystem.TryGetSolution(mob, mobFoodComponent.Solution, out var mobFoodSolution))
                return false;

            // This line here is mainly for mice, because they have a food
            // component that mirrors how much blood they have, which is
            // used for the reagent grinder.
            if (alreadyHadFood)
                _solutionContainerSystem.RemoveAllSolution(mobFoodSolution.Value);

            if (TryComp<BloodstreamComponent>(mob, out var bloodstreamComponent) && bloodstreamComponent.ChemicalSolution != null)
            {
                // Fry off any blood into protein.
                var bloodSolution = bloodstreamComponent.BloodSolution;
                var solPresent = bloodSolution!.Value.Comp.Solution.Volume;
                _solutionContainerSystem.RemoveReagent(bloodSolution.Value, "Blood", FixedPoint2.MaxValue);
                var bloodRemoved = solPresent - bloodSolution.Value.Comp.Solution.Volume;

                var proteinQuantity = bloodRemoved * BloodToProteinRatio;
                mobFoodSolution.Value.Comp.Solution.MaxVolume += proteinQuantity;
                _solutionContainerSystem.TryAddReagent(mobFoodSolution.Value, "Protein", proteinQuantity);

                // This is a heuristic. If you had blood, you might just taste meaty.
                if (bloodRemoved > FixedPoint2.Zero)
                    EnsureComp<FlavorProfileComponent>(mob).Flavors.Add(MobFlavorMeat);

                // Bring in whatever chemicals they had in them too.
                mobFoodSolution.Value.Comp.Solution.MaxVolume +=
                    bloodstreamComponent.ChemicalSolution.Value.Comp.Solution.Volume;
                _solutionContainerSystem.AddSolution(mobFoodSolution.Value,
                    bloodstreamComponent.ChemicalSolution.Value.Comp.Solution);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Make an item actually edible.
    /// </summary>
    private void MakeEdible(EntityUid uid, DeepFryerComponent component, EntityUid item, FixedPoint2 solutionQuantity)
    {
        if (!TryComp<DeepFriedComponent>(item, out var deepFriedComponent))
        {
            _sawmill.Error($"{ToPrettyString(item)} is missing the DeepFriedComponent before being made Edible.");
            return;
        }

        // Remove any components that wouldn't make sense anymore.
        RemComp<ButcherableComponent>(item);

        if (TryComp<PaperComponent>(item, out var paperComponent))
        {
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < paperComponent.Content.Length; ++i)
            {
                var uchar = paperComponent.Content.Substring(i, 1);

                if (uchar == "\n" || _random.Prob(0.4f))
                    stringBuilder.Append(uchar);
                else
                    stringBuilder.Append("x");
            }

            paperComponent.Content = stringBuilder.ToString();
        }

        var foodComponent = EnsureComp<FoodComponent>(item);
        var extraSolution = new Solution();
        if (TryComp(item, out FlavorProfileComponent? flavorProfileComponent))
        {
            HashSet<string> goodFlavors = new(flavorProfileComponent.Flavors);
            goodFlavors.IntersectWith(component.GoodFlavors);

            HashSet<string> badFlavors = new(flavorProfileComponent.Flavors);
            badFlavors.IntersectWith(component.BadFlavors);

            deepFriedComponent.PriceCoefficient = Math.Max(0.01f,
                1.0f
                + goodFlavors.Count * component.GoodFlavorPriceBonus
                - badFlavors.Count * component.BadFlavorPriceMalus);

            if (goodFlavors.Count > 0)
            {
                foreach (var reagent in component.GoodReagents)
                {
                    extraSolution.AddReagent(reagent.Reagent.ToString(), reagent.Quantity * goodFlavors.Count);

                    // Mask the taste of "medicine."
                    flavorProfileComponent.IgnoreReagents.Add(reagent.Reagent.ToString());
                }
            }

            if (badFlavors.Count > 0)
            {
                foreach (var reagent in component.BadReagents)
                {
                    extraSolution.AddReagent(reagent.Reagent.ToString(), reagent.Quantity * badFlavors.Count);
                }
            }
        }
        else
        {
            flavorProfileComponent = EnsureComp<FlavorProfileComponent>(item);
            // TODO: Default flavor?
        }

        // Make sure there's enough room for the fryer solution.
        var foodSolution = _solutionContainerSystem.EnsureSolution(item, foodComponent.Solution);
        if (!_solutionContainerSystem.TryGetSolution(item, foodSolution.Name, out var foodContainer))
            return;

        // The solution quantity is used to give the fried food an extra
        // buffer too, to support injectables or condiments.
        foodSolution.MaxVolume = 2 * solutionQuantity + foodSolution.Volume + extraSolution.Volume;
        _solutionContainerSystem.AddSolution(foodContainer.Value,
            component.Solution.SplitSolution(solutionQuantity));
        _solutionContainerSystem.AddSolution(foodContainer.Value, extraSolution);
        _solutionContainerSystem.UpdateChemicals(foodContainer.Value);
    }
}
