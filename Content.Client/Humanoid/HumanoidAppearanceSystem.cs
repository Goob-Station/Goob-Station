// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.DisplacementMap;
using Content.Shared.CCVar;
using Content.Shared.CCVar;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Preferences;
using Robust.Client.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Humanoid;

public sealed class HumanoidAppearanceSystem : SharedHumanoidAppearanceSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly DisplacementMapSystem _displacement = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HumanoidAppearanceComponent, AfterAutoHandleStateEvent>(OnHandleState);
        Subs.CVar(_configurationManager, CCVars.AccessibilityClientCensorNudity, OnCvarChanged, true);
        Subs.CVar(_configurationManager, CCVars.AccessibilityServerCensorNudity, OnCvarChanged, true);
    }

    private void OnHandleState(EntityUid uid, HumanoidAppearanceComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateSprite((uid, component, Comp<SpriteComponent>(uid)));
    }

    private void OnCvarChanged(bool value)
    {
        var humanoidQuery = EntityManager.AllEntityQueryEnumerator<HumanoidAppearanceComponent, SpriteComponent>();
        while (humanoidQuery.MoveNext(out var _, out var humanoidComp, out var spriteComp))
        {
            UpdateSprite(humanoidComp, spriteComp);
        }
    }

    private void UpdateSprite(HumanoidAppearanceComponent component, SpriteComponent sprite)
    {
        var humanoidQuery = AllEntityQuery<HumanoidAppearanceComponent, SpriteComponent>();
        while (humanoidQuery.MoveNext(out var uid, out var humanoidComp, out var spriteComp))
        {
            UpdateSprite((uid, humanoidComp, spriteComp));
        }
    }

    public void UpdateSprite(Entity<HumanoidAppearanceComponent, SpriteComponent> entity) // Goob edit - made public
    {
        UpdateLayers(entity);
        ApplyMarkingSet(entity);

        var humanoidAppearance = entity.Comp1;
        var sprite = entity.Comp2;

        // begin Goobstation: port EE height/width sliders
        var speciesPrototype = _prototypeManager.Index<SpeciesPrototype>(humanoidAppearance.Species);

        // Pirate start - port Floofstation custom layers
        //var height = Math.Clamp(humanoidAppearance.Height, speciesPrototype.MinHeight, speciesPrototype.MaxHeight);
        //var width = Math.Clamp(humanoidAppearance.Width, speciesPrototype.MinWidth, speciesPrototype.MaxWidth);
        var height = humanoidAppearance.Height != 0
            ? Math.Clamp(humanoidAppearance.Height, speciesPrototype.MinHeight, speciesPrototype.MaxHeight)
            : speciesPrototype.MinHeight;
        var width = humanoidAppearance.Width != 0
            ? Math.Clamp(humanoidAppearance.Width, speciesPrototype.MinWidth, speciesPrototype.MaxWidth)
            : speciesPrototype.MinWidth;
        // Pirate end - port Floofstation custom layers
        humanoidAppearance.Height = height;
        humanoidAppearance.Width = width;

        _sprite.SetScale((entity, sprite), new Vector2(width, height));
        // end Goobstation: port EE height/width sliders

        sprite[_sprite.LayerMapReserve((entity.Owner, sprite), HumanoidVisualLayers.Eyes)].Color = humanoidAppearance.EyeColor;
    }

    private static bool IsHidden(HumanoidAppearanceComponent humanoid, HumanoidVisualLayers layer)
        => humanoid.HiddenLayers.ContainsKey(layer) || humanoid.PermanentlyHidden.Contains(layer);

    private void UpdateLayers(Entity<HumanoidAppearanceComponent, SpriteComponent> entity)
    {
        var component = entity.Comp1;
        var sprite = entity.Comp2;

        var oldLayers = new HashSet<HumanoidVisualLayers>(component.BaseLayers.Keys);
        component.BaseLayers.Clear();

        // add default species layers
        var speciesProto = _prototypeManager.Index(component.Species);
        var baseSprites = _prototypeManager.Index(speciesProto.SpriteSet);
        foreach (var (key, id) in baseSprites.Sprites)
        {
            oldLayers.Remove(key);
            if (!component.CustomBaseLayers.ContainsKey(key))
                SetLayerData(entity, key, id, sexMorph: true);
        }

        // add custom layers
        foreach (var (key, info) in component.CustomBaseLayers)
        {
            oldLayers.Remove(key);
            SetLayerData(entity, key, info.Id, sexMorph: false, color: info.Color, overrideSkin: true, shader: info.Shader);
        }

        // hide old layers
        // TODO maybe just remove them altogether?
        foreach (var key in oldLayers)
        {
            if (_sprite.LayerMapTryGet((entity.Owner, sprite), key, out var index, false))
                sprite[index].Visible = false;
        }
    }

    private void SetLayerData(
        Entity<HumanoidAppearanceComponent, SpriteComponent> entity,
        HumanoidVisualLayers key,
        string? protoId,
        bool sexMorph = false,
        Color? color = null,
        bool overrideSkin = false,
        string? shader = null) // Shitmed Change // Goob edit
    {
        var component = entity.Comp1;
        var sprite = entity.Comp2;

        var layerIndex = _sprite.LayerMapReserve((entity.Owner, sprite), key);
        var layer = sprite[layerIndex];
        layer.Visible = !IsHidden(component, key);

        if (color != null)
            layer.Color = color.Value;

        if (shader != null) // Goobstation
            sprite.LayerSetShader(layerIndex, shader);
        else
            sprite.LayerSetShader(layerIndex, null, null);

        if (protoId == null)
            return;

        if (sexMorph)
            protoId = HumanoidVisualLayersExtension.GetSexMorph(key, component.Sex, protoId);

        var proto = _prototypeManager.Index<HumanoidSpeciesSpriteLayer>(protoId);
        component.BaseLayers[key] = proto;

        if (proto.MatchSkin && !overrideSkin) // Shitmed Change
            layer.Color = component.SkinColor.WithAlpha(proto.LayerAlpha);

        if (proto.BaseSprite != null)
            _sprite.LayerSetSprite((entity.Owner, sprite), layerIndex, proto.BaseSprite);
    }

    /// <summary>
    ///     Loads a profile directly into a humanoid.
    /// </summary>
    /// <param name="uid">The humanoid entity's UID</param>
    /// <param name="profile">The profile to load.</param>
    /// <param name="humanoid">The humanoid entity's humanoid component.</param>
    /// <remarks>
    ///     This should not be used if the entity is owned by the server. The server will otherwise
    ///     override this with the appearance data it sends over.
    /// </remarks>
    public override void LoadProfile(EntityUid uid, HumanoidCharacterProfile? profile, HumanoidAppearanceComponent? humanoid = null)
    {
        if (profile == null)
            return;

        if (!Resolve(uid, ref humanoid))
        {
            return;
        }

        var customBaseLayers = new Dictionary<HumanoidVisualLayers, CustomBaseLayerInfo>();

        var speciesPrototype = _prototypeManager.Index<SpeciesPrototype>(profile.Species);
        var markings = new MarkingSet(speciesPrototype.MarkingPoints, _markingManager, _prototypeManager);

        // Add markings that doesn't need coloring. We store them until we add all other markings that doesn't need it.
        var markingFColored = new Dictionary<Marking, MarkingPrototype>();
        foreach (var marking in profile.Appearance.Markings)
        {
            if (_markingManager.TryGetMarking(marking, out var prototype))
            {
                if (!prototype.ForcedColoring)
                {
                    markings.AddBack(prototype.MarkingCategory, marking);
                }
                else
                {
                    markingFColored.Add(marking, prototype);
                }
            }
        }

        // legacy: remove in the future?
        //markings.RemoveCategory(MarkingCategories.Hair);
        //markings.RemoveCategory(MarkingCategories.FacialHair);

        // We need to ensure hair before applying it or coloring can try depend on markings that can be invalid
        var hairColor = _markingManager.MustMatchSkin(profile.Species, HumanoidVisualLayers.Hair, out var hairAlpha, _prototypeManager)
            ? profile.Appearance.SkinColor.WithAlpha(hairAlpha)
            : profile.Appearance.HairColor;
        var hair = new Marking(profile.Appearance.HairStyleId,
            new[] { hairColor });

        var facialHairColor = _markingManager.MustMatchSkin(profile.Species, HumanoidVisualLayers.FacialHair, out var facialHairAlpha, _prototypeManager)
            ? profile.Appearance.SkinColor.WithAlpha(facialHairAlpha)
            : profile.Appearance.FacialHairColor;
        var facialHair = new Marking(profile.Appearance.FacialHairStyleId,
            new[] { facialHairColor });

        if (_markingManager.CanBeApplied(profile.Species, profile.Sex, hair, _prototypeManager))
        {
            markings.AddBack(MarkingCategories.Hair, hair);
        }
        if (_markingManager.CanBeApplied(profile.Species, profile.Sex, facialHair, _prototypeManager))
        {
            markings.AddBack(MarkingCategories.FacialHair, facialHair);
        }

        // Finally adding marking with forced colors
        foreach (var (marking, prototype) in markingFColored)
        {
            var markingColors = MarkingColoring.GetMarkingLayerColors(
                prototype,
                profile.Appearance.SkinColor,
                profile.Appearance.EyeColor,
                markings
            );
            markings.AddBack(prototype.MarkingCategory, new Marking(marking.MarkingId, markingColors));
        }

        markings.EnsureSpecies(profile.Species, profile.Appearance.SkinColor, _markingManager, _prototypeManager);
        markings.EnsureSexes(profile.Sex, _markingManager);
        markings.EnsureDefault(
            profile.Appearance.SkinColor,
            profile.Appearance.EyeColor,
            _markingManager);

        DebugTools.Assert(IsClientSide(uid));

        humanoid.MarkingSet = markings;
        humanoid.PermanentlyHidden = new HashSet<HumanoidVisualLayers>();
        humanoid.HiddenLayers = new Dictionary<HumanoidVisualLayers, SlotFlags>();
        humanoid.CustomBaseLayers = customBaseLayers;
        humanoid.Sex = profile.Sex;
        humanoid.Gender = profile.Gender;
        humanoid.Age = profile.Age;
        humanoid.Species = profile.Species;
        humanoid.SkinColor = profile.Appearance.SkinColor;
        humanoid.EyeColor = profile.Appearance.EyeColor;
        humanoid.Height = profile.Height; // Goobstation: port EE height/width sliders
        humanoid.Width = profile.Width; // Goobstation: port EE height/width sliders

        UpdateSprite((uid, humanoid, Comp<SpriteComponent>(uid)));
    }

    private void ApplyMarkingSet(Entity<HumanoidAppearanceComponent, SpriteComponent> entity)
    {
        var humanoid = entity.Comp1;
        var sprite = entity.Comp2;

        // I am lazy and I CBF resolving the previous mess, so I'm just going to nuke the markings.
        // Really, markings should probably be a separate component altogether.
        ClearAllMarkings(entity);

        var censorNudity = _configurationManager.GetCVar(CCVars.AccessibilityClientCensorNudity) ||
                           _configurationManager.GetCVar(CCVars.AccessibilityServerCensorNudity);
        // The reason we're splitting this up is in case the character already has undergarment equipped in that slot.
        var applyUndergarmentTop = censorNudity;
        var applyUndergarmentBottom = censorNudity;

        foreach (var markingList in humanoid.MarkingSet.Markings.Values)
        {
            foreach (var marking in markingList)
            {
                if (_markingManager.TryGetMarking(marking, out var markingPrototype))
                {
                    ApplyMarking(markingPrototype, marking.MarkingColors, marking.Visible, entity);
                    if (markingPrototype.BodyPart == HumanoidVisualLayers.UndergarmentTop)
                        applyUndergarmentTop = false;
                    else if (markingPrototype.BodyPart == HumanoidVisualLayers.UndergarmentBottom)
                        applyUndergarmentBottom = false;
                }
            }
        }

        humanoid.ClientOldMarkings = new MarkingSet(humanoid.MarkingSet);

        AddUndergarments(entity, applyUndergarmentTop, applyUndergarmentBottom);
    }

    private void ClearAllMarkings(Entity<HumanoidAppearanceComponent, SpriteComponent> entity)
    {
        var humanoid = entity.Comp1;
        var sprite = entity.Comp2;

        foreach (var markingList in humanoid.ClientOldMarkings.Markings.Values)
        {
            foreach (var marking in markingList)
            {
                RemoveMarking(marking, (entity, sprite));
            }
        }

        humanoid.ClientOldMarkings.Clear();

        foreach (var markingList in humanoid.MarkingSet.Markings.Values)
        {
            foreach (var marking in markingList)
            {
                RemoveMarking(marking, (entity, sprite));
            }
        }
    }

    private void RemoveMarking(Marking marking, Entity<SpriteComponent> spriteEnt)
    {
        if (!_markingManager.TryGetMarking(marking, out var prototype))
        {
            return;
        }

        foreach (var sprite in prototype.Sprites)
        {
            if (sprite is not SpriteSpecifier.Rsi rsi)
            {
                continue;
            }

            var layerId = $"{marking.MarkingId}-{rsi.RsiState}";
            if (!_sprite.LayerMapTryGet(spriteEnt.AsNullable(), layerId, out var index, false))
            {
                continue;
            }

            _sprite.LayerMapRemove(spriteEnt.AsNullable(), layerId);
            _sprite.RemoveLayer(spriteEnt.AsNullable(), index);
        }
    }

    private void AddUndergarments(Entity<HumanoidAppearanceComponent, SpriteComponent> entity, bool undergarmentTop, bool undergarmentBottom)
    {
        var humanoid = entity.Comp1;

        if (undergarmentTop && humanoid.UndergarmentTop != null)
        {
            var marking = new Marking(humanoid.UndergarmentTop, new List<Color> { new Color() });
            if (_markingManager.TryGetMarking(marking, out var prototype))
            {
                // Markings are added to ClientOldMarkings because otherwise it causes issues when toggling the feature on/off.
                humanoid.ClientOldMarkings.Markings.Add(MarkingCategories.UndergarmentTop, new List<Marking> { marking });
                ApplyMarking(prototype, null, true, entity);
            }
        }

        if (undergarmentBottom && humanoid.UndergarmentBottom != null)
        {
            var marking = new Marking(humanoid.UndergarmentBottom, new List<Color> { new Color() });
            if (_markingManager.TryGetMarking(marking, out var prototype))
            {
                humanoid.ClientOldMarkings.Markings.Add(MarkingCategories.UndergarmentBottom, new List<Marking> { marking });
                ApplyMarking(prototype, null, true, entity);
            }
        }
    }

    // Pirate start - port Floofstation custom layers
    // This function is heavily modified to support FloofStation's layering and colorLinks.
    private void ApplyMarking(MarkingPrototype markingPrototype,
        IReadOnlyList<Color>? colors,
        bool visible,
        Entity<HumanoidAppearanceComponent, SpriteComponent> entity)
    {
        var humanoid = entity.Comp1;
        var sprite = entity.Comp2;

        var colorDict = new Dictionary<string, Color>();
        for (var i = 0; i < markingPrototype.Sprites.Count; i++)
        {
            var spriteName = markingPrototype.Sprites[i] switch
            {
                SpriteSpecifier.Rsi rsi => rsi.RsiState,
                SpriteSpecifier.Texture texture => texture.TexturePath.Filename,
                _ => null
            };

            if (spriteName != null)
            {
                var color = (colors != null && i < colors.Count)
                    ? colors[i]
                    : Color.White;
                colorDict[spriteName] = color;
            }
        }

        if (markingPrototype.ColorLinks != null)
        {
            foreach (var (child, parent) in markingPrototype.ColorLinks)
            {
                if (child == null || parent == null)
                    continue;

                if (colorDict.TryGetValue(parent, out var color))
                {
                    colorDict[child] = color;
                }
            }
        }

        var layerDict = new Dictionary<string, int>();

        if (!_sprite.LayerMapTryGet((entity.Owner, sprite), markingPrototype.BodyPart, out var targetLayer, false))
        {
            return;
        }

        visible &= !IsHidden(humanoid, markingPrototype.BodyPart);
        visible &= humanoid.BaseLayers.TryGetValue(markingPrototype.BodyPart, out var setting)
           && setting.AllowsMarkings;

        for (var j = 0; j < markingPrototype.Sprites.Count; j++)
        {
            var markingSprite = markingPrototype.Sprites[j];

            if (markingSprite is not SpriteSpecifier.Rsi rsi)
            {
                continue;
            }

            var layerId = $"{markingPrototype.ID}-{rsi.RsiState}";

            var layerSlot = markingPrototype.BodyPart;
            var currentTargetIndex = 0;

            // Pirate - Floofstation layering support
            if (markingPrototype.Layering != null &&
                markingPrototype.Layering.TryGetValue(rsi.RsiState, out var layerValue))
            {
                var layerKey = layerValue?.ToString();

                if (layerKey != null && Enum.TryParse<HumanoidVisualLayers>(layerKey, true, out var layerEnum))
                {
                    layerSlot = layerEnum;
                }
            }

            if (!_sprite.LayerMapTryGet((entity.Owner, sprite), layerSlot, out var customLayerIndex, false))
            {
                layerSlot = markingPrototype.BodyPart;
                currentTargetIndex = targetLayer;
            }
            else
            {
                currentTargetIndex = customLayerIndex;
            }

            if (layerDict.TryGetValue(layerSlot.ToString(), out var layerIndex))
            {
                layerDict[layerSlot.ToString()] = layerIndex + 1;
            }
            else
            {
                layerDict.Add(layerSlot.ToString(), 0);
            }

            var targLayerAdj = currentTargetIndex + layerDict[layerSlot.ToString()] + 1;

            if (!_sprite.LayerMapTryGet((entity.Owner, sprite), layerId, out _, false))
            {
                var layer = _sprite.AddLayer((entity.Owner, sprite), markingSprite, targLayerAdj);
                _sprite.LayerMapSet((entity.Owner, sprite), layerId, layer);
                _sprite.LayerSetSprite((entity.Owner, sprite), layerId, rsi);
            }

            // Goobstation - get CustomBaseLayers info for shader/color fallback
            var hasInfo = humanoid.CustomBaseLayers.TryGetValue(markingPrototype.BodyPart, out var info);

            // impstation/Goobstation - shader handling with CustomBaseLayers fallback
            if (markingPrototype.Shader != null)
            {
                sprite.LayerSetShader(layerId, markingPrototype.Shader);
            }
            else if (hasInfo && info.Shader != null)
            {
                sprite.LayerSetShader(layerId, info.Shader);
            }
            else
            {
                if (_sprite.LayerMapTryGet((entity.Owner, sprite), layerId, out var shaderLayerIndex, false)) // Pirate - Floof Station layering
                    sprite.LayerSetShader(shaderLayerIndex, null, null); // Pirate - Floof Station layering
            }

            _sprite.LayerSetVisible((entity.Owner, sprite), layerId, visible);

            if (!visible || setting == null) // this is kinda implied
            {
                continue;
            }

            // Pirate - use colorDict (respects ColorLinks), then Goobstation interpolation with info.Color
            var spriteColor = colorDict.TryGetValue(rsi.RsiState, out var dictColor) ? dictColor : Color.White;
            if (hasInfo && info.Color != null)
                spriteColor = Color.InterpolateBetween(spriteColor, info.Color.Value, 0.5f);
            _sprite.LayerSetColor((entity.Owner, sprite), layerId, spriteColor);

            if (humanoid.MarkingsDisplacement.TryGetValue(markingPrototype.BodyPart, out var displacementData) && markingPrototype.CanBeDisplaced)
            {
                _displacement.TryAddDisplacement(displacementData, (entity.Owner, sprite), targLayerAdj, layerId, out _);
            }
        }
    }
    // Pirate end - port Floofstation custom layers

    public override void SetSkinColor(EntityUid uid, Color skinColor, bool sync = true, bool verify = true, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid) || humanoid.SkinColor == skinColor)
            return;

        base.SetSkinColor(uid, skinColor, false, verify, humanoid);

        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        foreach (var (layer, spriteInfo) in humanoid.BaseLayers)
        {
            if (!spriteInfo.MatchSkin)
                continue;

            var index = _sprite.LayerMapReserve((uid, sprite), layer);
            sprite[index].Color = skinColor.WithAlpha(spriteInfo.LayerAlpha);
        }
    }

    public override void SetLayerVisibility(
        Entity<HumanoidAppearanceComponent> ent,
        HumanoidVisualLayers layer,
        bool visible,
        SlotFlags? slot,
        ref bool dirty)
    {
        base.SetLayerVisibility(ent, layer, visible, slot, ref dirty);

        var sprite = Comp<SpriteComponent>(ent);
        if (!_sprite.LayerMapTryGet((ent.Owner, sprite), layer, out var index, false))
        {
            if (!visible)
                return;
            index = _sprite.LayerMapReserve((ent.Owner, sprite), layer);
        }

        var spriteLayer = sprite[index];
        if (spriteLayer.Visible == visible)
            return;

        spriteLayer.Visible = visible;

        // I fucking hate this. I'll get around to refactoring sprite layers eventually I swear
        // Just a week away...

        foreach (var markingList in ent.Comp.MarkingSet.Markings.Values)
        {
            foreach (var marking in markingList)
            {
                if (_markingManager.TryGetMarking(marking, out var markingPrototype) && markingPrototype.BodyPart == layer)
                    ApplyMarking(markingPrototype, marking.MarkingColors, marking.Visible, (ent, ent.Comp, sprite));
            }
        }
    }
}
