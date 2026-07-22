// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using System.Text;
using Content.Shared._Shitmed.StatusEffects;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.Prototypes;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Storage.Components;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Item; // Goobstation - anythingburgers
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Interaction.Components; // Goobstation - anythingburgers

namespace Content.Shared.Nutrition.EntitySystems;

public sealed class FoodSequenceSystem : SharedFoodSequenceSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IngestionSystem _ingestion = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    [Dependency] private readonly SharedItemSystem _item = default!; // Goobstation - anythingburgers
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FoodSequenceStartPointComponent, InteractUsingEvent>(OnInteractUsing);

        SubscribeLocalEvent<FoodMetamorphableByAddingComponent, FoodSequenceIngredientAddedEvent>(OnIngredientAdded);
    }

    private void OnInteractUsing(Entity<FoodSequenceStartPointComponent> ent, ref InteractUsingEvent args)
    {
         if (HasComp<EntityStorageComponent>(args.Used)
             || HasComp<StorageComponent>(args.Used)
             || HasComp<UnremoveableComponent>(args.Used)) // Goobstation - Prevent burgering unremovable items
             return; // Prevent Backpacks/Pet Carriers

         if (ent.Comp.AcceptAll) // Goobstation - anythingburgers
             EnsureComp<FoodSequenceElementComponent>(args.Used);

         if (TryComp<FoodSequenceElementComponent>(args.Used, out var sequenceElement) && HasComp<ItemComponent>(args.Used) && !HasComp<FoodSequenceStartPointComponent>(args.Used)) // Goobstation - anythingburgers - no non items allowed! otherwise you can grab players and lockers and such and add them to burgers
             args.Handled = TryAddFoodElement(ent, (args.Used, sequenceElement), args.User);
    }

    private void OnIngredientAdded(Entity<FoodMetamorphableByAddingComponent> ent, ref FoodSequenceIngredientAddedEvent args)
    {
        if (!TryComp<FoodSequenceStartPointComponent>(args.Start, out var start))
            return;

        if (!_proto.Resolve(args.Proto, out var elementProto))
            return;

        if (!ent.Comp.OnlyFinal || elementProto.Final || start.FoodLayers.Count == start.MaxLayers)
        {
            TryMetamorph((ent, start));
        }
    }

    private bool TryMetamorph(Entity<FoodSequenceStartPointComponent> start)
    {
        List<MetamorphRecipePrototype> availableRecipes = new();
        foreach (var recipe in _proto.EnumeratePrototypes<MetamorphRecipePrototype>())
        {
            if (recipe.Key != start.Comp.Key)
                continue;

            bool allowed = true;
            foreach (var rule in recipe.Rules)
            {
                if (!rule.Check(_proto, EntityManager, start, start.Comp.FoodLayers))
                {
                    allowed = false;
                    break;
                }
            }
            if (allowed)
                availableRecipes.Add(recipe);
        }

        if (availableRecipes.Count <= 0)
            return true;

        Metamorf(start, _random.Pick(availableRecipes)); //In general, if there's more than one recipe, the yml-guys screwed up. Maybe some kind of unit test is needed.
        PredictedQueueDel(start.Owner);
        return true;
    }

    private void Metamorf(Entity<FoodSequenceStartPointComponent> start, MetamorphRecipePrototype recipe)
    {
        var result = PredictedSpawnNextToOrDrop(recipe.Result, start);

        //Try putting in container
        _transform.DropNextTo(result, (start, Transform(start)));

        if (!_solutionContainer.TryGetSolution(result, start.Comp.Solution, out var resultSoln, out var resultSolution))
            return;

        if (!_solutionContainer.TryGetSolution(start.Owner, start.Comp.Solution, out var startSoln, out var startSolution))
            return;

        _solutionContainer.RemoveAllSolution(resultSoln.Value); //Remove all YML reagents
        resultSoln.Value.Comp.Solution.MaxVolume = startSoln.Value.Comp.Solution.MaxVolume;
        _solutionContainer.TryAddSolution(resultSoln.Value, startSolution);

        MergeFlavorProfiles(start, result);
        MergeTrash(start.Owner, result);
        MergeTags(start, result);
    }

    private bool TryAddFoodElement(Entity<FoodSequenceStartPointComponent> start, Entity<FoodSequenceElementComponent, EdibleComponent?> element, EntityUid? user = null) // trauma
    {
        // we can't add a live mouse to a burger.
        // <Goob> don't care if the burger accepts anything
        if (!start.Comp.AcceptAll)
        {
            if (!Resolve(element, ref element.Comp2))
                return false;
            if (element.Comp2.RequireDead && _mobState.IsAlive(element))
                return false;
        }

        // </Goob>
        // <Trauma>
        // i cba making my own fixes for this
        //looking for a suitable FoodSequence prototype
        if (!element.Comp1.Entries.TryGetValue(start.Comp.Key, out var elementProto))
            return false;

        if (!_proto.Resolve(elementProto, out var elementIndexed))
            return false;
        // </Trauma>
        //if we run out of space, we can still put in one last, final finishing element.
        if (start.Comp.FoodLayers.Count >= start.Comp.MaxLayers && !elementIndexed.Final || start.Comp.Finished)
        {
            if (user is not null)
                _popup.PopupClient(Loc.GetString("food-sequence-no-space"), start, user.Value);
            return false;
        }

        // Prevents plushies with items hidden in them from being added to prevent deletion of items
        // If more of these types of checks need to be added, this should be changed to an event or something.
        if (TryComp<SecretStashComponent>(element, out var stashComponent) && stashComponent.ItemContainer.Count != 0)
        {
            return false;
        }

        //Generate new visual layer
        var flip = start.Comp.AllowHorizontalFlip && _random.Prob(0.5f);
        var layer = new FoodSequenceVisualLayer(elementIndexed,
            _random.Pick(elementIndexed.Sprites),
            new Vector2(flip ? -elementIndexed.Scale.X : elementIndexed.Scale.X, elementIndexed.Scale.Y),
            new Vector2(
                _random.NextFloat(start.Comp.MinLayerOffset.X, start.Comp.MaxLayerOffset.X),
                _random.NextFloat(start.Comp.MinLayerOffset.Y, start.Comp.MaxLayerOffset.Y))
        );

        start.Comp.FoodLayers.Add(layer);
        Dirty(start);

        if (elementIndexed.Final)
            start.Comp.Finished = true;

        UpdateFoodName(start);
        UpdateFoodSize(start); // Goobstation - anythingburgers
        MergeFoodSolutions(start.Owner, element.Owner);
        MergeFlavorProfiles(start, element);
        MergeTrash(start.Owner, element.Owner);
        MergeTags(start, element);

        var ev = new FoodSequenceIngredientAddedEvent(start, element, elementProto, user);
        RaiseLocalEvent(start, ev);

        PredictedQueueDel(element.Owner);
        return true;
    }

    private void UpdateFoodName(Entity<FoodSequenceStartPointComponent> start)
    {
        if (start.Comp.NameGeneration is null)
            return;

        var content = new StringBuilder();
        var separator = "";
        if (start.Comp.ContentSeparator is not null)
            separator = Loc.GetString(start.Comp.ContentSeparator);

        HashSet<ProtoId<FoodSequenceElementPrototype>> existedContentNames = new();
        foreach (var layer in start.Comp.FoodLayers)
        {
            if (!existedContentNames.Contains(layer.Proto))
                existedContentNames.Add(layer.Proto);
        }

        var nameCounter = 1;
        foreach (var proto in existedContentNames)
        {
            if (!_proto.Resolve(proto, out var protoIndexed))
                continue;

            if (protoIndexed.Name is null)
                continue;

            content.Append(Loc.GetString(protoIndexed.Name.Value));

            if (nameCounter < existedContentNames.Count)
                content.Append(separator);
            nameCounter++;
        }

        var newName = Loc.GetString(start.Comp.NameGeneration.Value,
            ("prefix", start.Comp.NamePrefix is not null ? Loc.GetString(start.Comp.NamePrefix) : ""),
            ("content", content),
            ("suffix", start.Comp.NameSuffix is not null ? Loc.GetString(start.Comp.NameSuffix) : ""));

        _metaData.SetEntityName(start, newName);
    }

    private void MergeFoodSolutions(Entity<EdibleComponent?> start, Entity<EdibleComponent?> elementFood)
    {
        if (!Resolve(start, ref start.Comp, false))
            return;

        if (!Resolve(elementFood, ref elementFood.Comp, false))
            return;

        if (!_solutionContainer.TryGetSolution(start.Owner, start.Comp.Solution, out var startSolutionEntity, out var startSolution))
            return;

        if (!_solutionContainer.TryGetSolution(elementFood.Owner, elementFood.Comp.Solution, out _, out var elementSolution))
            return; // Goob anythingburg

        if (TryComp<SolutionContainerManagerComponent>(elementFood, out var elementSolutionContainer)) // Goobstation - anythingburgers We don't give a FUCK if the solution container is food or not, and i dont see why you woold.
        {
            foreach (var name in elementSolutionContainer.Containers)
            {
                if (!_solutionContainer.TryGetSolution(elementFood.Owner, name, out _, out var elementSolutionGoob))
                    continue;

                startSolution.MaxVolume += elementSolutionGoob.MaxVolume;
                _solutionContainer.TryAddSolution(startSolutionEntity.Value, elementSolutionGoob);
            }
        }
    }


    private void MergeFlavorProfiles(EntityUid start, EntityUid element)
    {
        if (!TryComp<FlavorProfileComponent>(start, out var startProfile))
            return;

        if (!TryComp<FlavorProfileComponent>(element, out var elementProfile))
            return;

        foreach (var flavor in elementProfile.Flavors)
        {
            if (startProfile != null && !startProfile.Flavors.Contains(flavor))
                startProfile.Flavors.Add(flavor);
        }
    }

    private void MergeTrash(Entity<EdibleComponent?> start, Entity<EdibleComponent?> element)
    {
        if (!Resolve(start, ref start.Comp, false))
            return;

        if (!Resolve(element, ref element.Comp, false))
            return;

        _ingestion.AddTrash((start, start.Comp), element.Comp.Trash);
    }

    private void MergeTags(EntityUid start, EntityUid element)
    {
        if (!TryComp<TagComponent>(element, out var elementTags))
            return;

        EnsureComp<TagComponent>(start);

        _tag.TryAddTags(start, elementTags.Tags);
    }

    private void UpdateFoodSize(Entity<FoodSequenceStartPointComponent> start) // Goobstation - anythingburgers dynamic item size
    {
        var increment = (start.Comp.FoodLayers.Count / 2);

        if (HasComp<ItemComponent>(start))
        {
            var sizeMap = new Dictionary<int, string>
            {
                { 1, "Small" },
                { 2, "Normal" },
                { 3, "Large" },
                { 4, "Huge" },
                { 5, "Ginormous" }
            };

            if (sizeMap.ContainsKey(increment))
            {
                _item.SetSize(start, sizeMap[increment]);
            }
            else if (increment == 6)
            {
                _transform.DropNextTo(start.Owner, start.Owner);
                RemComp<ItemComponent>(start);
            }

            _item.SetShape(start, new List<Box2i> { new Box2i(0, 0, 1, increment) });
        }
        // todo goob refactor this or move GravityWellComponent to shared
        // This kinda works but the teleport afterwards is kinda ass so replace or kill
        // you cant do this anyway with most things due to limit on stacking
        else if (increment >= 8) {
            EnsureComp<SpawnGravityWellComponent>(start, out var gravityWell);
            gravityWell.MaxRange = (float)Math.Sqrt(increment/4);
            gravityWell.BaseRadialAcceleration = (float)Math.Sqrt(increment/4);
        }
    }
}
