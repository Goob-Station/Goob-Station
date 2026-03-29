using System.Linq;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
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
        ent.Comp.SpawnTime = _timing.CurTime;
    }

    private void OnInteraction(Entity<TimedReplaceComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!TryComp<EggFertilizerComponent>(args.User, out var user))
            return;

        var doAfter =
            new DoAfterArgs(EntityManager, args.User, user.DoAfter, new FertilizeDoAfterEvent(), ent.Owner)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
            };

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
        EntProtoId? eggToLay = null;

        if (!TryComp<MostRecentlyEatenFoodTagsComponent>(ent.Owner, out var foodTags)
            || !TryComp<HappinessComponent>(ent.Owner, out var happiness))
            return;

        var sortedRecipes = _proto.EnumeratePrototypes<EggRecipePrototype>()
            .OrderByDescending(p => p.RequiresSpecialFood)
            .ThenByDescending(p => p.HappinessRequired);

        var currentHappiness = _happiness.GetHappiness((ent.Owner, happiness));

        if (currentHappiness is null)
            return;

        var entityPrototype = MetaData(ent.Owner).EntityPrototype;

        if (entityPrototype is null)
            return;

        foreach (var proto in sortedRecipes)
        {
            int requiredHappiness;

            requiredHappiness = proto.HappinessRequired;

            if (proto.ChickensRequireDifferentHappiness is not null)
            {
                foreach (var chicken in proto.ChickensRequireDifferentHappiness)
                {
                    if (chicken.Key == entityPrototype.ID)
                    {
                        requiredHappiness = chicken.Value;
                        break;
                    }
                }
            }

            if (requiredHappiness > currentHappiness)
                continue;

            var hascomps = true;

            if (proto.ComponentsRequired is not null)
            {
                foreach (var (name, reg) in proto.ComponentsRequired)
                {
                    if (!HasComp(ent.Owner, reg.Component.GetType()))
                        hascomps = false;
                }
            }

            if (!hascomps)
                continue;

            bool chickenAccepted = false;

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

            if (!proto.RequiresSpecialFood)
            {
                eggToLay = proto.Egg;
                break;
            }

            foreach (var chicken in proto.NoSpecialFoodRequiredChickens)
            {
                if (chicken == entityPrototype.ID)
                {
                    eggToLay = proto.Egg;
                    break;
                }
            }

            if (foodTags.Tag is null)
                continue;

            foreach (var tag in foodTags.Tag)
            {
                if (proto.FoodTagsRequired is not null && proto.FoodTagsRequired.Contains(tag))
                {
                    eggToLay = proto.Egg;
                    break;
                }
            }

            if (eggToLay is not null)
                break;
        }

        if (eggToLay is null)
            return;

        ent.Comp.EggSpawn = eggToLay;

        var ev = new RanchingEggLayEvent(ent);
        RaiseLocalEvent(ent.Owner, ref ev);
    }
}
