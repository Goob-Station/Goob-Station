using System.Numerics;
using System.Text;
using Content.Server.Nutrition.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Nutrition.Prototypes;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Item; // Goobstation - anythingburgers
using Content.Shared.Chemistry.Components.SolutionManager; // Goobstation - anythingburgers
using Content.Server.Singularity.Components; // Goobstation - anythingburgers
using Content.Shared.Singularity.Components; // Goobstation - anythingburgers

namespace Content.Server.Nutrition.EntitySystems;

public sealed class FoodSequenceSystem : SharedFoodSequenceSystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedItemSystem _item = default!; // Goobstation - anythingburgers
    [Dependency] private readonly SharedTransformSystem _transform = default!; // Goobstation - anythingburgers

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FoodSequenceStartPointComponent, InteractUsingEvent>(OnInteractUsing);

        SubscribeLocalEvent<FoodMetamorphableByAddingComponent, FoodSequenceIngredientAddedEvent>(OnIngredientAdded);
    }

    private void OnInteractUsing(Entity<FoodSequenceStartPointComponent> ent, ref InteractUsingEvent args)
    {
        if (ent.Comp.AcceptAll) // Goobstation - anythingburgers
            EnsureComp<FoodSequenceElementComponent>(args.Used);

        if (TryComp<FoodSequenceElementComponent>(args.Used, out var sequenceElement) && HasComp<ItemComponent>(args.Used) && !HasComp<FoodSequenceStartPointComponent>(args.Used)) // Goobstation - anythingburgers - no non items allowed! otherwise you can grab players and lockers and such and add them to burgers
            TryAddFoodElement(ent, (args.Used, sequenceElement), args.User);
    }

    private void OnIngredientAdded(Entity<FoodMetamorphableByAddingComponent> ent, ref FoodSequenceIngredientAddedEvent args)
    {
        if (!TryComp<FoodSequenceStartPointComponent>(args.Start, out var start))
            return;

        if (!_proto.TryIndex(args.Proto, out var elementProto))
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
        QueueDel(start);
        return true;
    }

    private void Metamorf(Entity<FoodSequenceStartPointComponent> start, MetamorphRecipePrototype recipe)
    {
        var result = SpawnAtPosition(recipe.Result, Transform(start).Coordinates);

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
        MergeTrash(start, result);
        MergeTags(start, result);
    }

    private bool TryAddFoodElement(Entity<FoodSequenceStartPointComponent> start, Entity<FoodSequenceElementComponent> element, EntityUid? user = null)
    {
        FoodSequenceElementEntry? elementData = null;
        foreach (var entry in element.Comp.Entries)
        {
            if (entry.Key == start.Comp.Key || start.Comp.AcceptAll) // Goobstation - anythingburgers
            {
                elementData = entry.Value;
                break;
            }
        }

        if (elementData is null)
            return false;

        if (TryComp<FoodComponent>(element, out var elementFood) && elementFood.RequireDead && !start.Comp.AcceptAll) // Goobstation - anythingburgers
        {
            if (_mobState.IsAlive(element))
                return false;
        }

        //if we run out of space, we can still put in one last, final finishing element.
        if (start.Comp.FoodLayers.Count >= start.Comp.MaxLayers && !elementIndexed.Final || start.Comp.Finished)
        {
            if (user is not null)
                _popup.PopupEntity(Loc.GetString("food-sequence-no-space"), start, user.Value);
            return false;
        }

        //Generate new visual layer
        var flip = start.Comp.AllowHorizontalFlip && _random.Prob(0.5f);
        var layer = new FoodSequenceVisualLayer(elementIndexed,
            _random.Pick(elementIndexed.Sprites),
            new Vector2(flip ? -1 : 1, 1),
            new Vector2(
                _random.NextFloat(start.Comp.MinLayerOffset.X, start.Comp.MaxLayerOffset.X),
                _random.NextFloat(start.Comp.MinLayerOffset.Y, start.Comp.MaxLayerOffset.Y))
        );

        elementData.LocalOffset = new Vector2(
            _random.NextFloat(start.Comp.MinLayerOffset.X, start.Comp.MaxLayerOffset.X),
            _random.NextFloat(start.Comp.MinLayerOffset.Y, start.Comp.MaxLayerOffset.Y));

        start.Comp.FoodLayers.Add(elementData);
        Dirty(start);

        if (elementIndexed.Final)
            start.Comp.Finished = true;

        UpdateFoodName(start);
        UpdateFoodSize(start); // Goobstation - anythingburgers
        MergeFoodSolutions(start, element);
        MergeFlavorProfiles(start, element);
        MergeTrash(start, element);
        MergeTags(start, element);

        if (!IsClientSide(element)) // Goobstation - anythingburgers PSEUDOITEMS FUCK THIS SHIT UP!!
            QueueDel(element);

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
            if (!_proto.TryIndex(proto, out var protoIndexed))
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

    private void MergeFoodSolutions(EntityUid start, EntityUid element)
    {
        if (!TryComp<FoodComponent>(start, out var startFood))
            return;

        if (TryComp<SolutionContainerManagerComponent>(element, out var elementSolutionContainer))
        { // Goobstation - anythingburgers We don't give a FUCK if the solution container is food or not, and i dont see why you woold.
            foreach (var name in elementSolutionContainer.Containers)
            {
                if (!_solutionContainer.TryGetSolution(element.Owner, name, out _, out var elementSolution))
                    continue;

                startSolution.MaxVolume += elementSolution.MaxVolume;
                _solutionContainer.TryAddSolution(startSolutionEntity.Value, elementSolution);
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

    private void MergeTrash(EntityUid start, EntityUid element)
    {
        if (!TryComp<FoodComponent>(start, out var startFood))
            return;

        if (!TryComp<FoodComponent>(element, out var elementFood))
            return;

        foreach (var trash in elementFood.Trash)
        {
            startFood.Trash.Add(trash);
        }
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
        } else if (increment >= 8) {
            EnsureComp<GravityWellComponent>(start, out var gravityWell);
            gravityWell.MaxRange = (float)Math.Sqrt(increment/4);
            gravityWell.BaseRadialAcceleration = (float)Math.Sqrt(increment/4);
        }
    }
}
