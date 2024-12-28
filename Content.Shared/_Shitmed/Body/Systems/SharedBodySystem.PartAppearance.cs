using System.Linq;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared._Shitmed.Body.Part;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Body.Systems;
public partial class SharedBodySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private void InitializePartAppearances()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyPartAppearanceComponent, ComponentStartup>(OnPartAppearanceStartup);
        SubscribeLocalEvent<BodyPartAppearanceComponent, AfterAutoHandleStateEvent>(HandleState);
        SubscribeLocalEvent<BodyComponent, BodyPartAddedEvent>(OnPartAttachedToBody);
        SubscribeLocalEvent<BodyComponent, BodyPartRemovedEvent>(OnPartDroppedFromBody);
    }

    private void OnPartAppearanceStartup(EntityUid uid, BodyPartAppearanceComponent component, ComponentStartup args)
    {
        if (!TryComp(uid, out BodyPartComponent? part)
            || part.ToHumanoidLayers() is not { } relevantLayer)
            return;

        if (part.BaseLayerId != null)
        {
            component.ID = part.BaseLayerId;
            component.Type = relevantLayer;
            return;
        }

        if (part.Body is not { Valid: true } body
            || !TryComp(body, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        var customLayers = bodyAppearance.CustomBaseLayers;
        var spriteLayers = bodyAppearance.BaseLayers;
        component.Type = relevantLayer;

        part.Species = bodyAppearance.Species;

        if (customLayers.ContainsKey(component.Type))
        {
            component.ID = customLayers[component.Type].Id;
            component.Color = customLayers[component.Type].Color;
        }
        else if (spriteLayers.ContainsKey(component.Type))
        {
            component.ID = spriteLayers[component.Type].ID;
            component.Color = bodyAppearance.SkinColor;
        }
        else
        {
            component.ID = CreateIdFromPart(bodyAppearance, relevantLayer);
            component.Color = bodyAppearance.SkinColor;
        }

        // I HATE HARDCODED CHECKS I HATE HARDCODED CHECKS I HATE HARDCODED CHECKS
        if (part.PartType == BodyPartType.Head)
            component.EyeColor = bodyAppearance.EyeColor;

        var markingsByLayer = new Dictionary<HumanoidVisualLayers, List<Marking>>();

        foreach (var layer in HumanoidVisualLayersExtension.Sublayers(relevantLayer))
        {
            var category = MarkingCategoriesConversion.FromHumanoidVisualLayers(layer);
            if (bodyAppearance.MarkingSet.Markings.TryGetValue(category, out var markingList))
                markingsByLayer[layer] = markingList.Select(m => new Marking(m.MarkingId, m.MarkingColors.ToList())).ToList();
        }

        component.Markings = markingsByLayer;
    }

    private string? CreateIdFromPart(HumanoidAppearanceComponent bodyAppearance, HumanoidVisualLayers part)
    {
        var speciesProto = _prototypeManager.Index(bodyAppearance.Species);
        var baseSprites = _prototypeManager.Index<HumanoidSpeciesBaseSpritesPrototype>(speciesProto.SpriteSet);

        if (!baseSprites.Sprites.ContainsKey(part))
            return null;

        return HumanoidVisualLayersExtension.GetSexMorph(part, bodyAppearance.Sex, baseSprites.Sprites[part]);
    }

    public void ModifyMarkings(EntityUid uid,
        Entity<BodyPartAppearanceComponent?> partAppearance,
        HumanoidAppearanceComponent bodyAppearance,
        HumanoidVisualLayers targetLayer,
        string markingId,
        bool remove = false)
    {

        if (!Resolve(partAppearance, ref partAppearance.Comp))
            return;

        if (!remove)
        {

            if (!_markingManager.Markings.TryGetValue(markingId, out var prototype))
                return;

            var markingColors = MarkingColoring.GetMarkingLayerColors(
                    prototype,
                    bodyAppearance.SkinColor,
                    bodyAppearance.EyeColor,
                    bodyAppearance.MarkingSet
                );

            var marking = new Marking(markingId, markingColors);

            _humanoid.SetLayerVisibility(uid, targetLayer, true, true, bodyAppearance);
            _humanoid.AddMarking(uid, markingId, markingColors, true, true, bodyAppearance);
            if (!partAppearance.Comp.Markings.ContainsKey(targetLayer))
                partAppearance.Comp.Markings[targetLayer] = new List<Marking>();

            partAppearance.Comp.Markings[targetLayer].Add(marking);
        }
        //else
            //RemovePartMarkings(uid, component, bodyAppearance);
    }

    private void HandleState(EntityUid uid, BodyPartAppearanceComponent component, ref AfterAutoHandleStateEvent args) =>
        ApplyPartMarkings(uid, component);

    private void OnPartAttachedToBody(EntityUid uid, BodyComponent component, ref BodyPartAddedEvent args)
    {
        if (!TryComp(args.Part, out BodyPartAppearanceComponent? partAppearance)
            || !TryComp(uid, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        if (partAppearance.ID != null)
            _humanoid.SetBaseLayerId(uid, partAppearance.Type, partAppearance.ID, sync: true, bodyAppearance);

        UpdateAppearance(uid, partAppearance);
    }

    private void OnPartDroppedFromBody(EntityUid uid, BodyComponent component, ref BodyPartRemovedEvent args)
    {
        if (TerminatingOrDeleted(uid)
            || TerminatingOrDeleted(args.Part)
            || !TryComp(uid, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        // We check for this conditional here since some entities may not have a profile... If they dont
        // have one, and their part is gibbed, the markings will not be removed or applied properly.
        if (!HasComp<BodyPartAppearanceComponent>(args.Part))
            EnsureComp<BodyPartAppearanceComponent>(args.Part);

        if (TryComp<BodyPartAppearanceComponent>(args.Part, out var partAppearance))
            RemoveAppearance(uid, partAppearance, args.Part);
    }

    protected void UpdateAppearance(EntityUid target,
        BodyPartAppearanceComponent component)
    {
        if (!TryComp(target, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        if (component.EyeColor != null)
        {
            bodyAppearance.EyeColor = component.EyeColor.Value;
            _humanoid.SetLayerVisibility(target, HumanoidVisualLayers.Eyes, true, true, bodyAppearance);
        }

        if (component.Color != null)
            _humanoid.SetBaseLayerColor(target, component.Type, component.Color, true, bodyAppearance);

        _humanoid.SetLayerVisibility(target, component.Type, true, true, bodyAppearance);

        foreach (var (visualLayer, markingList) in component.Markings)
        {
            _humanoid.SetLayerVisibility(target, visualLayer, true, true, bodyAppearance);
            foreach (var marking in markingList)
            {
                _humanoid.AddMarking(target, marking.MarkingId, marking.MarkingColors, true, true, bodyAppearance);
            }
        }

        Dirty(target, bodyAppearance);
    }

    protected void RemoveAppearance(EntityUid entity, BodyPartAppearanceComponent component, EntityUid partEntity)
    {
        if (!TryComp(entity, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        foreach (var (visualLayer, markingList) in component.Markings)
        {
            _humanoid.SetLayerVisibility(entity, visualLayer, false, true, bodyAppearance);
        }
        RemoveBodyMarkings(entity, component, bodyAppearance);
    }

    protected abstract void ApplyPartMarkings(EntityUid target, BodyPartAppearanceComponent component);

    protected abstract void RemoveBodyMarkings(EntityUid target, BodyPartAppearanceComponent partAppearance, HumanoidAppearanceComponent bodyAppearance);
}
