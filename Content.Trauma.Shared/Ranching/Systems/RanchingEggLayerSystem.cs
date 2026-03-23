using System.Linq;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Trauma.Shared.Ranching.Components;
using Content.Trauma.Shared.Ranching.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

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
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RanchingEggLayerComponent, RanchingEggLayAttemptEvent>(OnEggLayAttempt);
        SubscribeLocalEvent<RanchingEggLayerComponent, RanchingEggLayEvent>(OnEggLay);
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

        if (foodTags.Tag is not null)
            foodTags.Tag.Clear();
    }

    private void OnEggLayAttempt(Entity<RanchingEggLayerComponent> ent, ref RanchingEggLayAttemptEvent args)
    {
        EntProtoId? eggToLay = null;

        if (!TryComp<MostRecentlyEatenFoodTagsComponent>(ent.Owner, out var foodTags)
            || !TryComp<HappinessComponent>(ent.Owner, out var happiness))
            return;

        var sortedRecipes = _proto.EnumeratePrototypes<EggRecipePrototype>()
            .OrderByDescending(p => p.HappinessRequired);

        var currentHappiness = _happiness.GetHappiness((ent.Owner, happiness));

        if (currentHappiness is null)
            return;

        Log.Debug(currentHappiness.ToString() ?? "poop");

        foreach (var proto in sortedRecipes)
        {
            if (proto.HappinessRequired > currentHappiness)
                continue;

            var entityPrototype = MetaData(ent.Owner).EntityPrototype;

            if (entityPrototype is null)
                continue;

            if (proto.RequiredChicken != entityPrototype.ID)
                continue;

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
