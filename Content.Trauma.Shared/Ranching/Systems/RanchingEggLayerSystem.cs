using System.Linq;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Content.Trauma.Shared.Ranching.Components;
using Content.Trauma.Shared.Ranching.Events;
using Content.Trauma.Shared.TimedReplace;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Trauma.Shared.Ranching.Systems;

/// <summary>
/// Handles the ranching egg layer system, use this for ranching instead of the upstream version so we don't have to fuck it up more than goob has and also ranching needs to change the eggs to a single proto, not a list
/// </summary>
public sealed class RanchingEggLayerSystem : EntitySystem
{

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly HappinessSystem _happiness = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RanchingEggLayerComponent, RanchingEggLayAttemptEvent>(OnEggLayAttempt);
        SubscribeLocalEvent<RanchingEggLayerComponent, RanchingEggLayEvent>(OnEggLay);

        SubscribeLocalEvent<TimedReplaceComponent, ActivateInWorldEvent>(OnInteraction);
        SubscribeLocalEvent<TimedReplaceComponent, FertilizeDoAfterEvent>(OnFertilize);
    }


    private void OnFertilize(Entity<TimedReplaceComponent> ent, ref FertilizeDoAfterEvent args)
    {
        if (args.Cancelled)
        {
            EnsureComp<EggFertilizationTargetComponent>(ent.Owner);
            return;
        }

        if (!TryComp<EggFertilizerComponent>(args.User, out var fertilizer) || !TryComp<HappinessComponent>(args.User, out var happiness))
            return;

        if (fertilizer.SpecialReplacement is null)
            ent.Comp.SpawnTime = _timing.CurTime;
        else
        {
            ent.Comp.Entity = fertilizer.SpecialReplacement.Value;
            ent.Comp.SpawnTime = _timing.CurTime;
            fertilizer.SpecialReplacement = null;
            _happiness.SetHappiness((args.User, happiness), 30f);
        }
    }

    private void OnInteraction(Entity<TimedReplaceComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!TryComp<EggFertilizerComponent>(args.User, out var user) || !HasComp<EggFertilizationTargetComponent>(ent.Owner))
            return;

        var doAfter =
            new DoAfterArgs(EntityManager, args.User, user.DoAfter, new FertilizeDoAfterEvent(), ent.Owner)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
            };

        RemComp<EggFertilizationTargetComponent>(ent.Owner);
        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnEggLay(Entity<RanchingEggLayerComponent> ent, ref RanchingEggLayEvent args)
    {
        SpawnNextToOrDrop(ent.Comp.EggSpawn, ent.Owner);

        _audio.PlayPvs(ent.Comp.EggLaySound, ent.Owner);
        _popup.PopupEntity(Loc.GetString("action-popup-lay-egg-user"), ent.Owner, ent.Owner);
        _popup.PopupEntity(Loc.GetString("action-popup-lay-egg-others", ("entity", ent.Owner)), ent.Owner, Filter.PvsExcept(ent.Owner), true);

        if (!TryComp<HungerComponent>(ent.Owner, out var hunger))
            return;

        _hunger.ModifyHunger(ent.Owner, -ent.Comp.HungerUsage, hunger);

        if (!TryComp<MostRecentlyEatenFoodTagsComponent>(ent.Owner, out var foodTags))
            return;

        foodTags.Tag.Clear();
    }

    private void OnEggLayAttempt(Entity<RanchingEggLayerComponent> ent, ref RanchingEggLayAttemptEvent args)
    {
        if (!TryComp<MostRecentlyEatenFoodTagsComponent>(ent.Owner, out var foodTags)
            || !TryComp<HappinessComponent>(ent.Owner, out var happiness))
            return;

        var currentHappiness = _happiness.GetHappiness((ent.Owner, happiness));
        if (currentHappiness is null)
            return;

        var entityPrototype = MetaData(ent.Owner).EntityPrototype;
        if (entityPrototype is null)
            return;

        var sortedRecipes = _proto.EnumeratePrototypes<EggRecipePrototype>()
            .OrderByDescending(p => p.ReagentsRequired is not null && p.ReagentsRequired.Count > 0)
            .ThenByDescending(p => p.FoodTagsRequired is not null && p.FoodTagsRequired.Count > 0)
            .ThenByDescending(p => p.Weight)
            .ThenByDescending(p => p.HappinessRequired);

        foreach (var proto in sortedRecipes)
        {
            var requiredHappiness = proto.HappinessRequired;
            if (proto.ChickensRequireDifferentHappiness is not null)
            {
                foreach (var kvp in proto.ChickensRequireDifferentHappiness)
                {
                    if (kvp.Key == entityPrototype.ID)
                    {
                        requiredHappiness = kvp.Value;
                        break;
                    }
                }
            }
            if (requiredHappiness > currentHappiness)
                continue;

            if (proto.ComponentsRequired is not null)
            {
                var hasAll = true;
                foreach (var (_, comp) in proto.ComponentsRequired)
                {
                    if (!HasComp(ent.Owner, comp.Component.GetType())) // Why is there no entitymananger.hascomponents
                    {
                        hasAll = false;
                        break;
                    }
                }
                if (!hasAll)
                    continue;
            }

            var chickenAccepted = false;
            foreach (var chicken in proto.RequiredChicken)
            {
                if (chicken == entityPrototype.ID)
                {
                    chickenAccepted = true;
                    break;
                }
            }
            if (!chickenAccepted)
                continue;

            if (proto.ReagentsRequired is not null && !HasRequiredReagent(ent, proto))
                continue;

            if (proto.FoodTagsRequired is null)
            {
                ent.Comp.EggSpawn = proto.Egg;
                var ev = new RanchingEggLayEvent(ent);
                RaiseLocalEvent(ent.Owner, ref ev);
                return;
            }

            var noFoodRequired = false;
            foreach (var chicken in proto.NoSpecialFoodRequiredChickens)
            {
                if (chicken == entityPrototype.ID)
                {
                    noFoodRequired = true;
                    break;
                }
            }

            if (noFoodRequired)
            {
                ent.Comp.EggSpawn = proto.Egg;
                var ev = new RanchingEggLayEvent(ent);
                RaiseLocalEvent(ent.Owner, ref ev);
                return;
            }

            if (foodTags.Tag is null)
                continue;

            foreach (var tag in foodTags.Tag)
            {
                if (!proto.FoodTagsRequired.Contains(tag))
                    continue;

                ent.Comp.EggSpawn = proto.Egg;
                var ev = new RanchingEggLayEvent(ent);
                RaiseLocalEvent(ent.Owner, ref ev);
                return;
            }
        }
    }

    private bool HasRequiredReagent(Entity<RanchingEggLayerComponent> ent, EggRecipePrototype proto)
    {
        if (proto.ReagentsRequired is null)
            return false;

        foreach (var reagent in proto.ReagentsRequired)
        {
            if (!HasComp<SolutionContainerManagerComponent>(ent.Owner))
                return false;

            if (!_solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out var bloodstream, out _))
                return false;

            if (!bloodstream.Value.Comp.Solution.ContainsPrototype(reagent.Key))
                return false;

            foreach (var (id, quantity) in bloodstream.Value.Comp.Solution.Contents)
            {
                if (id.Prototype != reagent.Key)
                    continue;

                if (quantity < reagent.Value)
                    return false;
            }
        }

        return true;
    }
}
